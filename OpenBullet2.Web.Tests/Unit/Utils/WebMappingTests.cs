using System.Text.Json;
using Mapster;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Blocks;
using OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;
using OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;
using OpenBullet2.Web.Dtos.Config.Blocks.Parameters;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Pagination;
using OpenBullet2.Web.Utils;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using RuriLib.Models.Settings;

namespace OpenBullet2.Web.Tests.Unit.Utils;

public class WebMappingTests
{
    [Fact]
    public void WebMapperConfig_Create_Compiles()
    {
        var config = WebMapperConfig.Create();

        config.Compile();
        Assert.NotNull(config);
    }

    [Fact]
    public void GuestMappings_PreservePasswordHashAndAllowedAddresses()
    {
        var mapper = CreateMapper();
        var createDto = new CreateGuestDto
        {
            Username = "guest",
            Password = "Password123!",
            AccessExpiration = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AllowedAddresses = ["127.0.0.1", "::1"]
        };

        var entity = mapper.Map<GuestEntity>(createDto);

        Assert.Equal("guest", entity.Username);
        Assert.Equal("127.0.0.1,::1", entity.AllowedAddresses);
        Assert.True(BCrypt.Net.BCrypt.Verify(createDto.Password, entity.PasswordHash));

        mapper.Map(new UpdateGuestInfoDto
        {
            Id = 1,
            Username = "guest2",
            AccessExpiration = new DateTime(2031, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AllowedAddresses = ["10.0.0.1"]
        }, entity);

        Assert.Equal("guest2", entity.Username);
        Assert.Equal("10.0.0.1", entity.AllowedAddresses);

        mapper.Map(new UpdateGuestPasswordDto
        {
            Id = 1,
            Password = "Password456!"
        }, entity);

        Assert.True(BCrypt.Net.BCrypt.Verify("Password456!", entity.PasswordHash));

        var dto = mapper.Map<GuestDto>(entity);

        Assert.Equal(["10.0.0.1"], dto.AllowedAddresses);
    }

    [Fact]
    public void SummaryMappings_ApplyDefaultsAndFlattenedFields()
    {
        var mapper = CreateMapper();
        var before = DateTime.Now;

        var hit = mapper.Map<HitEntity>(new CreateHitDto
        {
            Data = "user:pass",
            CapturedData = "token=abc",
            Type = "SUCCESS"
        });

        var after = DateTime.Now;

        Assert.Equal(string.Empty, hit.Proxy);
        Assert.Equal(string.Empty, hit.ConfigName);
        Assert.Equal(string.Empty, hit.ConfigCategory);
        Assert.Equal(string.Empty, hit.WordlistName);
        Assert.InRange(hit.Date, before, after);

        var proxyDto = mapper.Map<ProxyDto>(new ProxyEntity
        {
            Id = 7,
            Host = "127.0.0.1",
            Port = 8080,
            Type = ProxyType.Http,
            Status = ProxyWorkingStatus.Working,
            Group = new ProxyGroupEntity { Id = 3, Name = "Group" }
        });

        Assert.Equal(3, proxyDto.GroupId);
        Assert.Equal("Group", proxyDto.GroupName);
        Assert.Null(proxyDto.LastChecked);
        Assert.Equal(ProxyQuality.Unknown, proxyDto.Quality);

        var wordlistDto = mapper.Map<WordlistDto>(new WordlistEntity
        {
            Id = 11,
            Name = "WL",
            FileName = "wl.txt",
            Purpose = "Combos",
            Total = 3_000_000_000L,
            Type = "EmailPass"
        });

        Assert.Equal("wl.txt", wordlistDto.FilePath);
        Assert.Equal("Combos", wordlistDto.Purpose);
        Assert.Equal("EmailPass", wordlistDto.WordlistType);
        Assert.Equal(3_000_000_000L, wordlistDto.LineCount);

        var configInfo = mapper.Map<ConfigInfoDto>(new Config
        {
            Id = "cfg",
            IsRemote = true,
            Mode = ConfigMode.Stack,
            Metadata = new ConfigMetadata
            {
                Name = "Config",
                Author = "Author",
                Category = "Category",
                Base64Image = "img",
                CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            Settings = new ConfigSettings
            {
                GeneralSettings = new RuriLib.Models.Configs.Settings.GeneralSettings
                {
                    SuggestedBots = 42
                },
                ProxySettings = new RuriLib.Models.Configs.Settings.ProxySettings
                {
                    UseProxies = true
                },
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default", "EmailPass"]
                }
            }
        });

        Assert.True(configInfo.IsRemote);
        Assert.Equal(ConfigMode.Stack, configInfo.Mode);
        Assert.True(configInfo.NeedsProxies);
        Assert.Equal(42, configInfo.SuggestedBots);
        Assert.Equal(["Default", "EmailPass"], configInfo.AllowedWordlistTypes);
    }

    [Fact]
    public void ProxyCheckJobOptions_Mappings_HandlePolymorphicMembers()
    {
        var mapper = CreateMapper();
        var createDto = new CreateProxyCheckJobDto
        {
            Name = "Proxy Check",
            Bots = 5,
            GroupId = 9,
            CheckOnlyUntested = false,
            TimeoutMilliseconds = 2500,
            UseProxyJudge = false,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "ok"
            },
            StartCondition = SerializePoly(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromMinutes(5)
            }),
            CheckOutput = SerializePoly(new DatabaseProxyCheckOutputOptionsDto())
        };

        var options = mapper.Map<ProxyCheckJobOptions>(createDto);

        Assert.Equal("Proxy Check", options.Name);
        Assert.Equal(5, options.Bots);
        Assert.False(options.UseProxyJudge);
        Assert.IsType<RelativeTimeStartCondition>(options.StartCondition);
        Assert.IsType<DatabaseProxyCheckOutputOptions>(options.CheckOutput);

        var dto = mapper.Map<ProxyCheckJobOptionsDto>(options);

        var startConditionDto = Assert.IsType<RelativeTimeStartConditionDto>(dto.StartCondition);
        Assert.Equal(TimeSpan.FromMinutes(5), startConditionDto.StartAfter);
        Assert.False(dto.UseProxyJudge);

        var checkOutputDto = Assert.IsType<DatabaseProxyCheckOutputOptionsDto>(dto.CheckOutput);
        Assert.Equal("databaseProxyCheckOutput", checkOutputDto.PolyTypeName);
    }

    [Fact]
    public void MultiRunJobOptions_Mappings_HandlePolymorphicMembers()
    {
        var mapper = CreateMapper();
        var createDto = new CreateMultiRunJobDto
        {
            Name = "Multi Run",
            ConfigId = "cfg-id",
            Bots = 12,
            NeverMarkProxiesAsBad = true,
            CacheHits = false,
            DataPool = SerializePoly(new RangeDataPoolOptionsDto
            {
                Start = 100,
                Amount = 50,
                Step = 2,
                Pad = true,
                WordlistType = "Default"
            }),
            ProxySources =
            [
                SerializePoly(new GroupProxySourceOptionsDto { GroupId = 7 }),
                SerializePoly(new RemoteProxySourceOptionsDto { Url = "https://example.com/proxies", DefaultType = ProxyType.Socks5 })
            ],
            HitOutputs =
            [
                SerializePoly(new DatabaseHitOutputOptionsDto()),
                SerializePoly(new TelegramBotHitOutputOptionsDto { Token = "token", ChatId = 123, OnlyHits = false })
            ],
            StartCondition = SerializePoly(new AbsoluteTimeStartConditionDto
            {
                StartAt = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc)
            })
        };

        var options = mapper.Map<MultiRunJobOptions>(createDto);

        Assert.Equal("cfg-id", options.ConfigId);
        Assert.True(options.NeverMarkProxiesAsBad);
        Assert.False(options.CacheHits);
        Assert.IsType<AbsoluteTimeStartCondition>(options.StartCondition);
        Assert.IsType<RangeDataPoolOptions>(options.DataPool);
        Assert.IsType<GroupProxySourceOptions>(options.ProxySources[0]);
        Assert.IsType<RemoteProxySourceOptions>(options.ProxySources[1]);
        Assert.IsType<DatabaseHitOutputOptions>(options.HitOutputs[0]);
        Assert.IsType<TelegramBotHitOutputOptions>(options.HitOutputs[1]);

        var dto = mapper.Map<MultiRunJobOptionsDto>(options);

        Assert.IsType<AbsoluteTimeStartConditionDto>(dto.StartCondition);
        Assert.IsType<RangeDataPoolOptionsDto>(dto.DataPool);
        Assert.IsType<GroupProxySourceOptionsDto>(dto.ProxySources[0]);
        Assert.IsType<TelegramBotHitOutputOptionsDto>(dto.HitOutputs[1]);
        Assert.True(dto.NeverMarkProxiesAsBad);
        Assert.False(dto.CacheHits);
    }

    [Fact]
    public void ConfigDataSettings_Mapping_RoundTripsRulesAndResources()
    {
        var mapper = CreateMapper();
        var settings = new DataSettings
        {
            AllowedWordlistTypes = ["A", "B"],
            UrlEncodeDataAfterSlicing = true,
            DataRules =
            [
                new SimpleDataRule
                {
                    SliceName = "USER",
                    Comparison = StringRule.Contains,
                    StringToCompare = "@",
                    CaseSensitive = false
                },
                new RegexDataRule
                {
                    SliceName = "PASS",
                    RegexToMatch = "^[a-z]+$",
                    Invert = true
                }
            ],
            Resources =
            [
                new LinesFromFileResourceOptions
                {
                    Name = "users",
                    Location = "users.txt",
                    LoopsAround = false
                },
                new RandomLinesFromFileResourceOptions
                {
                    Name = "tokens",
                    Location = "tokens.txt",
                    IgnoreEmptyLines = false,
                    Unique = true
                }
            ]
        };

        var dto = mapper.Map<ConfigDataSettingsDto>(settings);

        Assert.Single(dto.DataRules.Simple);
        Assert.Single(dto.DataRules.Regex);
        Assert.Single(dto.Resources.LinesFromFile);
        Assert.Single(dto.Resources.RandomLinesFromFile);

        var mappedBack = mapper.Map<DataSettings>(dto);

        Assert.Equal(["A", "B"], mappedBack.AllowedWordlistTypes);
        Assert.True(mappedBack.UrlEncodeDataAfterSlicing);
        Assert.Equal(2, mappedBack.DataRules.Count);
        Assert.Equal(2, mappedBack.Resources.Count);
        Assert.Contains(mappedBack.DataRules, r => r is SimpleDataRule);
        Assert.Contains(mappedBack.DataRules, r => r is RegexDataRule);
        Assert.Contains(mappedBack.Resources, r => r is LinesFromFileResourceOptions);
        Assert.Contains(mappedBack.Resources, r => r is RandomLinesFromFileResourceOptions);
    }

    [Fact]
    public void TriggeredAction_Mappings_HandlePolymorphismAndUpdates()
    {
        var mapper = CreateMapper();
        var config = WebMapperConfig.Create();
        var createDto = new CreateTriggeredActionDto
        {
            Name = "Action",
            JobId = 12,
            IsRepeatable = true,
            Triggers =
            [
                SerializePoly(new JobStatusTriggerDto { Status = RuriLib.Models.Jobs.JobStatus.Running }),
                SerializePoly(new TimeElapsedTriggerDto
                {
                    Comparison = NumComparison.GreaterThan,
                    TimeSpan = TimeSpan.FromSeconds(30)
                })
            ],
            Actions =
            [
                SerializePoly(new StopJobActionDto { JobId = 12 }),
                SerializePoly(new WaitActionDto { TimeSpan = TimeSpan.FromSeconds(5) }),
                SerializePoly(new SetSkipActionDto { Skip = 250 })
            ]
        };

        var action = mapper.Map<TriggeredAction>(createDto);

        Assert.Equal("Action", action.Name);
        Assert.Equal(12, action.JobId);
        Assert.Equal(2, action.Triggers.Count);
        Assert.Equal(3, action.Actions.Count);
        Assert.IsType<JobStatusTrigger>(action.Triggers[0]);
        Assert.IsType<TimeElapsedTrigger>(action.Triggers[1]);
        Assert.IsType<WaitAction>(action.Actions[1]);
        Assert.IsType<SetSkipAction>(action.Actions[2]);

        action.Executions = 4;
        var dto = mapper.Map<TriggeredActionDto>(action);

        Assert.Equal(4, dto.Executions);
        Assert.Equal(2, dto.Triggers.Count);
        Assert.Equal(3, dto.Actions.Count);
        Assert.IsType<JobStatusTriggerDto>(dto.Triggers[0]);
        Assert.IsType<WaitActionDto>(dto.Actions[1]);
        Assert.IsType<SetSkipActionDto>(dto.Actions[2]);

        var updated = WebMappingMethods.ApplyTriggeredAction(new UpdateTriggeredActionDto
        {
            Id = action.Id,
            Name = "Updated",
            JobId = 99,
            IsActive = false,
            Triggers = [SerializePoly(new ProgressTriggerDto { Comparison = NumComparison.EqualTo, Amount = 50 })],
            Actions = [SerializePoly(new StartJobActionDto { JobId = 99 })]
        }, action, config);

        Assert.Equal(action.Id, updated.Id);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal(99, updated.JobId);
        Assert.False(updated.IsActive);
        Assert.Single(updated.Triggers);
        Assert.Single(updated.Actions);
        Assert.IsType<ProgressTrigger>(updated.Triggers[0]);
        Assert.IsType<StartJobAction>(updated.Actions[0]);
    }

    [Fact]
    public void BlockSettingMapper_MapsFixedAndInterpolatedValues()
    {
        var stringSetting = BlockSettingFactory.CreateStringSetting("text", "hello");
        var stringDto = BlockSettingMapper.ToDto(stringSetting);

        Assert.Equal(BlockSettingType.String, stringDto.Type);
        Assert.Equal("hello", stringDto.Value);

        var byteArraySetting = BlockSettingFactory.CreateByteArraySetting("bytes", [1, 2, 3]);
        var byteArrayDto = BlockSettingMapper.ToDto(byteArraySetting);

        Assert.Equal(BlockSettingType.ByteArray, byteArrayDto.Type);
        Assert.Equal([1, 2, 3], Assert.IsType<byte[]>(byteArrayDto.Value));

        var listSetting = BlockSettingFactory.CreateListOfStringsSetting(
            "items", ["one", "two"], SettingInputMode.Interpolated);
        var listDto = BlockSettingMapper.ToDto(listSetting);

        Assert.Equal(BlockSettingType.ListOfStrings, listDto.Type);
        Assert.Equal(SettingInputMode.Interpolated, listDto.InputMode);
        Assert.Equal(["one", "two"], Assert.IsType<List<string>>(listDto.Value));

        var target = BlockSettingFactory.CreateDictionaryOfStringsSetting("headers");
        BlockSettingMapper.Apply(new BlockSettingDto
        {
            Name = "headers",
            Type = BlockSettingType.DictionaryOfStrings,
            InputMode = SettingInputMode.Fixed,
            Value = JsonSerializer.SerializeToElement(
                new Dictionary<string, string> { ["a"] = "1" }, Globals.JsonOptions)
        }, target);

        var value = Assert.IsType<DictionaryOfStringsSetting>(target.FixedSetting).Value;
        Assert.Equal("1", value["a"]);

        var bytesTarget = BlockSettingFactory.CreateByteArraySetting("bytes");
        BlockSettingMapper.Apply(new BlockSettingDto
        {
            Name = "bytes",
            Type = BlockSettingType.ByteArray,
            InputMode = SettingInputMode.Fixed,
            Value = JsonSerializer.SerializeToElement("AQID", Globals.JsonOptions)
        }, bytesTarget);

        Assert.Equal([1, 2, 3], Assert.IsType<ByteArraySetting>(bytesTarget.FixedSetting).Value);

        BlockSettingMapper.Apply(new BlockSettingDto
        {
            Name = "bytes",
            Type = BlockSettingType.ByteArray,
            InputMode = SettingInputMode.Fixed,
            Value = JsonSerializer.SerializeToElement<string?>(null, Globals.JsonOptions)
        }, bytesTarget);

        Assert.Empty(Assert.IsType<ByteArraySetting>(bytesTarget.FixedSetting).Value!);
    }

    [Fact]
    public void BlockMappings_HandleManualBlockBranches()
    {
        var mapper = CreateMapper();
        var descriptor = new HttpRequestBlockDescriptor();
        var block = new HttpRequestBlockInstance(descriptor)
        {
            Safe = true,
            RequestParams = new MultipartRequestParams
            {
                Boundary = BlockSettingFactory.CreateStringSetting("boundary", "abc"),
                Contents =
                [
                    new StringHttpContentSettingsGroup
                    {
                        Name = BlockSettingFactory.CreateStringSetting("name", "file"),
                        ContentType = BlockSettingFactory.CreateStringSetting("contentType", "text/plain"),
                        Data = BlockSettingFactory.CreateStringSetting("data", "payload")
                    }
                ]
            }
        };

        var dto = Assert.IsType<HttpRequestBlockInstanceDto>(
            mapper.Map<BlockInstanceDto>(block));

        Assert.Equal(BlockInstanceType.HttpRequest, dto.Type);
        var requestParamsDto = Assert.IsType<MultipartRequestParamsDto>(dto.RequestParams);
        Assert.Equal("abc", requestParamsDto.Boundary!.Value);
        Assert.IsType<StringHttpContentSettingsGroupDto>(requestParamsDto.Contents[0]);

        var stack = new List<JsonElement>
        {
            JsonSerializer.SerializeToElement(new KeycheckBlockInstanceDto
            {
                Id = "Keycheck",
                Keychains =
                [
                    new KeychainDto
                    {
                        ResultStatus = "SUCCESS",
                        Keys =
                        [
                            SerializePoly(new StringKeyDto
                            {
                                Comparison = StrComparison.Contains,
                                Left = NewStringSettingDto("left", "abc"),
                                Right = NewStringSettingDto("right", "b")
                            })
                        ]
                    }
                ]
            }, Globals.JsonOptions)
        };

        var mapped = stack.MapStack();
        var keycheck = Assert.IsType<KeycheckBlockInstance>(mapped[0]);
        var key = Assert.IsType<StringKey>(keycheck.Keychains[0].Keys[0]);

        Assert.Equal(StrComparison.Contains, key.Comparison);
        Assert.Equal("abc", Assert.IsType<StringSetting>(key.Left.FixedSetting).Value);
        Assert.Equal("b", Assert.IsType<StringSetting>(key.Right.FixedSetting).Value);
    }

    [Fact]
    public void DescriptorAndPagedListMappings_CoverManualHelpers()
    {
        var mapper = CreateMapper();
        var descriptorDto = mapper.Map<BlockDescriptorDto>(new HttpRequestBlockDescriptor());

        Assert.True(descriptorDto.Parameters.ContainsKey("method"));
        var methodParam = Assert.IsType<EnumBlockParameterDto>(descriptorDto.Parameters["method"]);
        Assert.Equal("HttpMethod", methodParam.Type);
        Assert.Equal("enumParam", methodParam.PolyTypeName);

        var paged = new PagedList<ProxyEntity>(
            [new ProxyEntity { Host = "127.0.0.1", Port = 80, Type = ProxyType.Http }],
            totalCount: 9,
            pageNumber: 2,
            pageSize: 4);

        var mapped = mapper.Map<PagedList<ProxyDto>>(paged);

        Assert.Equal(9, mapped.TotalCount);
        Assert.Equal(2, mapped.PageNumber);
        Assert.Equal(4, mapped.PageSize);
        Assert.Single(mapped.Items);
        Assert.Equal("127.0.0.1", mapped.Items[0].Host);
    }

    [Fact]
    public void SettingsApplyMethods_PreserveNestedObjectReferences()
    {
        var mapper = CreateMapper();
        var destination = new OpenBulletSettings();
        var general = destination.GeneralSettings;
        var remote = destination.RemoteSettings;
        var security = destination.SecuritySettings;
        var customization = destination.CustomizationSettings;

        WebMappingMethods.ApplyOpenBulletSettings(new OpenBulletSettingsDto
        {
            GeneralSettings = new OBGeneralSettingsDto
            {
                DefaultAuthor = "Author"
            },
            RemoteSettings = new RemoteSettings
            {
                ConfigsEndpoints =
                [
                    new RemoteConfigsEndpoint
                    {
                        Url = "https://repo.example"
                    }
                ]
            },
            SecuritySettings = new OBSecuritySettingsDto
            {
                AdminUsername = "admin2"
            },
            CustomizationSettings = new OBCustomizationSettingsDto
            {
                Theme = "Ocean"
            }
        }, destination, mapper);

        Assert.Same(general, destination.GeneralSettings);
        Assert.Same(remote, destination.RemoteSettings);
        Assert.Same(security, destination.SecuritySettings);
        Assert.Same(customization, destination.CustomizationSettings);
        Assert.Equal("Author", destination.GeneralSettings.DefaultAuthor);
        Assert.Single(destination.RemoteSettings.ConfigsEndpoints);
        Assert.Equal("https://repo.example", destination.RemoteSettings.ConfigsEndpoints[0].Url);
        Assert.Equal("admin2", destination.SecuritySettings.AdminUsername);
        Assert.Equal("Ocean", destination.CustomizationSettings.Theme);
    }

    private static IObjectMapper CreateMapper()
    {
        var config = WebMapperConfig.Create();
        return new MapsterObjectMapper(config);
    }

    private static JsonElement SerializePoly<T>(T dto) where T : OpenBullet2.Web.Dtos.PolyDto
    {
        dto.PolyTypeName = OpenBullet2.Web.Attributes.PolyTypeAttribute
            .FromType(dto.GetType())!.PolyType;

        return JsonSerializer.SerializeToElement(dto, Globals.JsonOptions);
    }

    private static BlockSettingDto NewStringSettingDto(string name, string value) => new()
    {
        Name = name,
        Type = BlockSettingType.String,
        InputMode = SettingInputMode.Fixed,
        Value = JsonSerializer.SerializeToElement(value, Globals.JsonOptions)
    };
}
