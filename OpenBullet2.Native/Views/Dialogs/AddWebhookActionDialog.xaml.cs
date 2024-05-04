using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddWebhookActionDialog.xaml
    /// </summary>
    public partial class AddWebhookActionDialog : Page
    {
        private TriggeredActionViewModel vm;
        private DiscordWebhookAction discordAction;
        private TelegramBotAction telegramAction;

        public AddWebhookActionDialog(TriggeredActionViewModel vm, DiscordWebhookAction discordAction)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
            this.discordAction = discordAction;
            webhookLabel.Content = "Discord Webhook Url";
            webhookText.Text = discordAction.Webhook;
            messageText.Text = discordAction.Message;
        }
        public AddWebhookActionDialog(TriggeredActionViewModel vm, TelegramBotAction telegramAction)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
            this.telegramAction = telegramAction;

            webhookChatId.Visibility = Visibility.Visible;
            webhookChatIdText.Visibility = Visibility.Visible;

            webhookLabel.Content = "Telegram Bot Token";
            webhookText.Text = telegramAction.Token;
            messageText.Text = telegramAction.Message;
            webhookChatIdText.Text = telegramAction.ChatId.ToString();
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            if (discordAction is not null)
            {
                discordAction.Webhook = webhookText.Text;
                discordAction.Message = messageText.Text;
            }
            else if(telegramAction is not null)
            {
                telegramAction.Token = webhookText.Text;
                telegramAction.Message = messageText.Text;
                telegramAction.ChatId = long.TryParse(webhookChatIdText.Text, out long chatId) ? chatId : 0;
            }

            ((MainDialog)Parent).Close();
        }
    }
}
