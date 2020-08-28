using System.Collections.Generic;

namespace Necromancy.Server.Data.Setting
{
    public class ModelCommonCsvReader : CsvReader<ModelCommonSetting>
    {
        private Dictionary<int, MonsterSetting> _monsterSetting;
        private Dictionary<int, ModelAtrSetting> _modelAtrSettings;

        public ModelCommonCsvReader(Dictionary<int, MonsterSetting> monsterSetting,
            Dictionary<int, ModelAtrSetting> modelAtrSettings)
        {
            _monsterSetting = monsterSetting;
            _modelAtrSettings = modelAtrSettings;
        }

        protected override int NumExpectedItems => 12;

        protected override ModelCommonSetting CreateInstance(string[] properties)
        {
            if (!int.TryParse(properties[0], out int id))
            {
                return null;
            }

            if (!int.TryParse(properties[1], out int radius))
            {
                return null;
            }

            if (!int.TryParse(properties[2], out int height))
            {
                return null;
            }

            if (!int.TryParse(properties[3], out int crouchHeight))
            {
                return null;
            }

            if (!int.TryParse(properties[4], out int nameHeight))
            {
                return null;
            }

            if (!int.TryParse(properties[5], out int modelAtrId))
            {
                return null;
            }

            if (!int.TryParse(properties[6], out int zRadiusOffset))
            {
                return null;
            }

            if (!int.TryParse(properties[7], out int effectScaling))
            {
                return null;
            }

            if (!int.TryParse(properties[8], out int active))
            {
                return null;
            }

            MonsterSetting monster = null;
            if (properties.Length > 12 && int.TryParse(properties[12], out int monsterId))
            {
                if (_monsterSetting.ContainsKey(monsterId))
                {
                    monster = _monsterSetting[monsterId];
                }
            }
            
            ModelAtrSetting atr = null;
            if (_modelAtrSettings.ContainsKey(modelAtrId))
            {
                atr = _modelAtrSettings[modelAtrId];
            }

            return new ModelCommonSetting
            {
                Id = id,
                Radius = radius,
                Height = height,
                CrouchHeight = crouchHeight,
                NameHeight = nameHeight,
                Atr = atr,
                ZRadiusOffset = zRadiusOffset,
                Effect = effectScaling,
                Active = active,
                Remarks = properties[10],
                Monster = monster
            };
        }
    }
}
