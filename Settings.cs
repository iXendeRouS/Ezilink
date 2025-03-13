using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;

namespace Ezilink
{
    public class Settings : ModSettings
    {
        public static readonly ModSettingBool EnableMod = new(false) { 
            description = "When enabled, link the buying, selling, and upgrading of all Ezilis."
        };
    }
}
