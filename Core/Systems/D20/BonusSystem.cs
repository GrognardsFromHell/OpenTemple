using System.Collections.Generic;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.D20
{
    public class BonusSystem
    {
        private readonly string _bonusLostDueToPrefix;
        private readonly string _bonusLostDueToSuffix;
        private readonly string _noPenaltyDueToPrefix;
        private readonly string _noPenaltyDueToSuffix;
        private readonly string _doesNotStackWithPrefix;
        private readonly string _doesNotStackWithSuffix;
        private readonly string _bonusReducedByPrefix;
        private readonly string _bonusReducedBySuffix;
        private readonly string _penaltyReducedByPrefix;
        private readonly string _penaltyReducedBySuffix;
        private readonly string _bonusCappedHighByPrefix;
        private readonly string _bonusCappedHighBySuffix;
        private readonly string _bonusCappedLowByPrefix;
        private readonly string _bonusCappedLowBySuffix;

        private readonly Dictionary<int, string> _bonusDescriptions;

        [TempleDllLocation(0x100e5eb0)]
        public BonusSystem()
        {
            var bonusMes = Tig.FS.ReadMesFile("mes/bonus.mes");

            _bonusLostDueToPrefix = bonusMes[0];
            _bonusLostDueToSuffix = bonusMes[1];
            _noPenaltyDueToPrefix = bonusMes[2];
            _noPenaltyDueToSuffix = bonusMes[3];
            _doesNotStackWithPrefix = bonusMes[4];
            _doesNotStackWithSuffix = bonusMes[5];
            _bonusReducedByPrefix = bonusMes[6];
            _bonusReducedBySuffix = bonusMes[7];
            _penaltyReducedByPrefix = bonusMes[8];
            _penaltyReducedBySuffix = bonusMes[9];
            _bonusCappedHighByPrefix = bonusMes[10];
            _bonusCappedHighBySuffix = bonusMes[11];
            _bonusCappedLowByPrefix = bonusMes[12];
            _bonusCappedLowBySuffix = bonusMes[13];

            // Merge the remaining bonus description lines with new ones from TemplePlus
            var bonusMesNew = Tig.FS.ReadMesFile("tpmes/bonus.mes");
            _bonusDescriptions = new Dictionary<int, string>(bonusMes.Count + bonusMesNew.Count);
            foreach (var (key, line) in bonusMes)
            {
                if (key <= 100)
                {
                    continue;
                }

                _bonusDescriptions[key] = line;
            }

            foreach (var (key, line) in bonusMesNew)
            {
                _bonusDescriptions[key] = line;
            }
        }

        public string GetBonusDescription(int lineKey) => _bonusDescriptions[lineKey];
    }
}