
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class Council
    {
        public static void initialize_council_events()
        {
            // sets persistent variable 'Varrr438' to store the time of the council
            if (ScriptDaemon.get_v(435) == 0)
            {
                ScriptDaemon.set_v(435, 1);
                var c_month = CurrentCalendar.Month + 1;
                var c_year = CurrentCalendar.Year;
                if (c_month == 13)
                {
                    c_month = 1;
                    c_year = c_year + 1;
                }

                ScriptDaemon.set_v(438, c_month + 32 * c_year);
            }

            return;
        }
        public static int council_time()
        {
            // returns 1 if the time is the first day of the month, between 22:00 and 22:30, the month after Burne said he'd address the council
            // returns 2 if it's on that day, between 22:30 and 23:00
            // returns 3 if it's between 19 and 22
            // returns 4 if it's after the coucil events have played out, and it isn't the time for an ordinary council
            // returns 5 if it's the time for an ordinary council
            // can be used to create council meetings in general!
            var currentCalendar = CurrentCalendar;
            var g_year = currentCalendar.Year;
            var g_month = currentCalendar.Month;
            var g_day = currentCalendar.Day;
            var g_hour = currentCalendar.Hour;
            var g_minute = currentCalendar.Minute;
            var c_month = (ScriptDaemon.get_v(438) & 31);
            var c_year = (ScriptDaemon.get_v(438) & (32768 - 1 - 31)) / 32; // uses a bit mask to filter the year, and brings it 5 bits down
                                                                            // explanation:
                                                                            // 32768 = 1000000000000000
                                                                            // 32768 - 1 = 1000000000000000 - 1 = 0111111111111111
                                                                            // 32768 - 1 - 31 = 111111111111111 - 11111 = 111111111100000
            if (ScriptDaemon.get_v(435) == 0)
            {
                c_year = 2097;
                if (g_year >= 2097)
                {
                    c_year = g_year + 1;
                }

            }

            if (g_year == c_year && g_month == c_month && g_day == 1 && g_hour == 22 && g_minute >= 0 && g_minute <= 30)
            {
                return 1;
            }
            else if ((g_year == c_year && g_month == c_month && g_day == 1 && g_hour == 22 && g_minute > 30))
            {
                return 2;
            }
            else if (g_month == c_month && g_day == 1 && g_hour >= 19 && g_hour < 22)
            {
                return 3;
            }
            else if ((g_year > c_year) || (g_year == c_year && g_month > c_month) || (g_year == c_year && g_month == c_month && g_day > 1) || (g_year == c_year && g_month == c_month && g_day == 1 && g_hour >= 23))
            {
                if (g_day == 1 && g_hour == 22 && g_minute >= 0 && g_minute <= 30)
                {
                    return 5;
                }

                return 4;
            }
            else if (g_day == 1 && g_hour == 22 && g_minute >= 0 && g_minute <= 30)
            {
                return 5;
            }

            return 0;
        }
        public static void council_heartbeat()
        {
            var c_time = council_time();
            // 1 - between 22:00 and 22:30 on council day
            // 2 - between 22:30 and 23:00
            // 3 - between 19:00 and 22:00
            // 4 - after council events ( >23:00 and beyond that day), but without ordinary council
            // 5 - ordinary council time
            // 0 - otherwise
            if (traders_awol() == 1)
            {
                ScriptDaemon.set_v(435, 6);
            }

            if (c_time == 5)
            {
                ScriptDaemon.set_v(440, 1);
            }
            else if (c_time == 1)
            {
                if (ScriptDaemon.get_v(435) == 1)
                {
                    ScriptDaemon.set_v(435, 3);
                }
                else if (ScriptDaemon.get_v(435) >= 5 || ScriptDaemon.get_v(435) == 0 && ScriptDaemon.get_v(440) == 0)
                {
                    ScriptDaemon.set_v(440, 1);
                }
                else if (ScriptDaemon.get_v(435) == 4)
                {
                    // council_events()
                    var dummy = 1;
                }

            }
            else if (c_time == 2)
            {
                if (ScriptDaemon.get_v(435) == 3 || ScriptDaemon.get_v(435) == 1)
                {
                    ScriptDaemon.set_v(435, 4);
                    ScriptDaemon.set_v(436, 1);
                    SetGlobalFlag(432, true);
                }

            }
            // council_events()
            else if (c_time == 3)
            {
                if (ScriptDaemon.get_v(435) == 2)
                {
                    ScriptDaemon.set_v(435, 5);
                    ScriptDaemon.set_v(436, 5);
                    SetGlobalVar(750, 3);
                    SetGlobalVar(751, 3);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

            }
            else if ((c_time == 0 || c_time == 4))
            {
                if (c_time == 0 && GetGlobalFlag(336) && (ScriptDaemon.get_v(435) == 1 || ScriptDaemon.get_v(435) == 2))
                {
                    ScriptDaemon.set_v(435, 0);
                }
                else if (ScriptDaemon.get_v(435) == 3 || ScriptDaemon.get_v(435) == 4 || (ScriptDaemon.get_v(435) == 1 && c_time == 4))
                {
                    // chiefly used for the case where the whole thing played out without you
                    if (ScriptDaemon.get_v(436) == 0)
                    {
                        ScriptDaemon.set_v(436, 1);
                    }

                    ScriptDaemon.set_v(435, 5);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

                if (ScriptDaemon.get_v(435) == 2 && c_time == 4)
                {
                    ScriptDaemon.set_v(435, 5);
                    ScriptDaemon.set_v(436, 5);
                    SetGlobalVar(750, 3);
                    SetGlobalVar(751, 3);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

                ScriptDaemon.set_v(440, 0);
            }

            return;
        }
        public static int tptc()
        {
            // time passage till council, in seconds
            // game.fade_and_teleport and game.fade use seconds in their passage of time fields
            if (ScriptDaemon.get_v(438) == 13)
            {
                var council_month = 1;
            }
            else
            {
                var council_month = ScriptDaemon.get_v(438) + 1;
            }

            var currentCalendar = CurrentCalendar;
            var this_month = currentCalendar.Month;
            var this_day = currentCalendar.Day;
            var this_hour = currentCalendar.Hour;
            var this_minute = currentCalendar.Minute;
            var ttw = 0;
            if (this_minute > 0)
            {
                ttw = ttw + (60 - this_minute) * 60;
                if (this_hour == 23)
                {
                    this_hour = 0;
                    if (this_day == 28)
                    {
                        this_day = 1;
                        if (this_month == 13)
                        {
                            this_month = 1;
                        }
                        else
                        {
                            this_month = this_month + 1;
                        }

                    }
                    else
                    {
                        this_day = this_day + 1;
                    }

                }
                else
                {
                    this_hour = this_hour + 1;
                }

            }

            bool TED;
            if (this_hour == 23)
            {
                this_hour = 0;
                ttw = ttw + 1 * (60) * 60;
                if (this_day == 28)
                {
                    TED = true;
                    this_day = 1;
                    if (this_month == 13)
                    {
                        this_month = 1;
                    }
                    else
                    {
                        this_month = this_month + 1;
                    }

                }
                else
                {
                    TED = false;
                    this_day = this_day + 1;
                }

            }
            else
            {
                TED = false;
            }

            ttw = ttw + (22 - this_hour) * 60 * 60;
            if (!TED)
            {
                ttw = ttw + (29 - this_day) * 24 * 60 * 60;
            }

            return ttw;
        }
        public static int traders_awol()
        {
            // this script determines whether the traders fled after being attacked -
            // the condition being that they surmise they are about to be exposed
            // it uses the time stamps to determine whether they knew this stuff BEFORE you assaulted them
            if (!GetGlobalFlag(426))
            {
                return 0;
            }

            var a = 0;
            if ((GetQuestState(15) == QuestState.Completed && ScriptDaemon.tsc(427, 426)))
            {
                // laborer spy revealed to Burne
                a = a + 1;
            }

            if ((GetQuestState(16) == QuestState.Completed && ScriptDaemon.tsc(431, 426)))
            {
                // confronted traders about laborer spy
                a = a + 1;
            }

            if (GetGlobalFlag(444) || GetGlobalFlag(422) || (GetGlobalFlag(428) && (GetGlobalFlag(7) || (CurrentTimeSeconds >= ScriptDaemon.get_v(423) + 24 * 60 * 60 && GetGlobalFlag(4)))))
            {
                // Discussed things with badger arrestor, or Confronted Traders about assassination attempt, with either presented hard evidence (Assassin's letter) or Corl was killed too
                a = a + 1;
            }

            if ((GetQuestState(17) == QuestState.Completed && ScriptDaemon.tsc(430, 426)))
            {
                // found out the courier
                a = a + 1;
            }

            if (a >= 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

    }
}
