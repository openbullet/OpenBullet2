using OpenBullet2.Models.Settings;
using System;

namespace OpenBullet2.Models.BlockParameters
{
    public abstract class BlockParameter
    {
        public string Name { get; set; }
        
        public virtual Setting ToSetting() 
        {
            throw new NotImplementedException(); 
        }
    }
}
