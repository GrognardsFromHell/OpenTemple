
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using System.Numerics;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class Itt
    {
        // obj_f_npc_pad_i_4 - stores earth critters Standpoint_ID
        // Note: using globals incurs a bug - globals are retained across saves...
        // i.e. I tried using global reserve_melee, it's fucked
        // And python apparently doesn't have static vars
        // Temple Level 1 regroup reserves

        private readonly struct Standpoint
        {
            public readonly locXY Location;
            public readonly int NameId;
            public readonly int Radius;

            public Standpoint(int x, int y, int nameId) : this(x, y, nameId, 1)
            {

            }

            public Standpoint(int x, int y, int nameId, int radius)
            {
                Location = new locXY(x, y);
                NameId = nameId;
                Radius = radius;
            }
        }

        private static readonly Standpoint[] bugbears_north_of_romag_melee = {
            new Standpoint(435, 424, 14163), 
            new Standpoint(430, 441, 14165), 
            new Standpoint(424, 435, 14165), 
            new Standpoint(432, 437, 14165)
        };
        private static readonly Standpoint[] bugbears_north_of_romag_ranged = {
            new Standpoint(423, 440, 14164),
            new Standpoint(433, 428, 14164)
        };
        private static readonly Standpoint[] earth_com_room_bugbears = {
            new Standpoint(452, 456, 14165),
            new Standpoint(446, 456, 14165),
            new Standpoint(471, 478, 14165),
            new Standpoint(465, 478, 14165)
        };
        private static readonly Standpoint[] turnkey_room = {
            new Standpoint(568, 462, 14165),
            new Standpoint(569, 458, 14229)
        };
        private static readonly Standpoint[] ogre_chief_rooms_melee = {
            new Standpoint(458, 532, 14078),
            new Standpoint(519, 533, 14078)
        };
        private static readonly Standpoint[] ogre_chief_rooms_ranged = {
            new Standpoint(470, 530, 14164),
            new Standpoint(520, 542, 1416)
        };
        public static void earth_reg()
        {
            var reserve_melee = new List<Standpoint>
            {
                new Standpoint(508, 477, 14162),
                new Standpoint(499, 476, 14165),
                new Standpoint(449, 449, 14163)
            };
            reserve_melee.AddRange(bugbears_north_of_romag_melee);
            reserve_melee.AddRange(turnkey_room);
            reserve_melee.AddRange(ogre_chief_rooms_melee);

            reserve_melee = trim_dead(reserve_melee);
            var reserve_ranged = trim_dead(bugbears_north_of_romag_ranged.Concat(ogre_chief_rooms_ranged));
            // see rnte() for parameter explanation
            // (          xxx , yyy , ID    , SP_ID,n_x , n_y , rot , dist, com,   reserve list )
            rnte(new locXY(523, 414), 14163, 650, 479, 387, 4.3f, 1, "melee", reserve_melee, out _); // Bugbear
            rnte(new locXY(440, 445), 14162, 651, 490, 383, 4.3f, 1, "melee", reserve_melee, out _); // Bugbear
            rnte(new locXY(505, 473), 14163, 652, 475, 394, 4.3f, 1, "melee", reserve_melee, out _); // Bugbear
            rnte(new locXY(517, 541), 14249, 658, 491, 389, 5.5f, 2, "big", new List<Standpoint>(), out _); // Ogre
            rnte(new locXY(498, 531), 14249, 659, 474, 388, 5.3f, 2, "big", new List<Standpoint>(), out _); // Ogre
            rnte(new locXY(508, 531), 14078, 660, 488, 381, 3.5f, 2, "melee", reserve_melee, out _); // Barbarian Gnoll
            rnte(new locXY(417, 441), 14163, 661, 497, 381, 5.4f, 2, "melee", reserve_melee, out _); // Sentry fore
            rnte(new locXY(441, 449), 14163, 662, 470, 384, 5.5f, 2, "melee", reserve_melee, out _); // Sentry fore
            rnte(new locXY(426, 432), 14162, 663, 493, 403, 5.45f, 2, "melee", reserve_melee, out _); // Sentry back
            rnte(new locXY(519, 416), 14162, 664, 471, 399, 5.54f, 2, "melee", reserve_melee, out _); // Sentry back
            rnte(new locXY(473, 404), 14337, 665, 480, 422, 4.7f, 4, "useless", new List<Standpoint>(), out _); // Earth robe guard
            rnte(new locXY(482, 396), 14337, 666, 493, 423, 6, 4, "useless", new List<Standpoint>(), out _); // Earth robe guard
            rnte(new locXY(489, 402), 14337, 667, 474, 433, 4.2f, 4, "useless", new List<Standpoint>(), out _); // Earth robe guard
            rnte(new locXY(505, 530), 14164, 669, 473, 405, 5.4f, 3, "ranged", reserve_ranged, out _); // Earth archer
            rnte(new locXY(502, 473), 14164, 670, 491, 397, 5.4f, 3, "ranged", reserve_ranged, out _); // Earth archer
            rnte(new locXY(475, 542), 14164, 671, 493, 432, 5.4f, 3, "ranged", reserve_ranged, out _); // Earth archer
            rnte(new locXY(446, 470), 14156, 683, 497, 386, 5.3f, 10, "special", new List<Standpoint>(), out _); // Commander
            rnte(new locXY(493, 391), 14381, 684, 495, 412, 5.4f, 2, "big", new List<Standpoint>(), out var earth_elemental_medium_1);
            rnte(new locXY(474, 392), 14381, 685, 470, 394, 4.3f, 2, "big", new List<Standpoint>(), out var earth_elemental_medium_2);
            if (earth_elemental_medium_1 != null)
            {
                earth_elemental_medium_1.SetFloat(obj_f.speed_run, 1.9514365f);
                earth_elemental_medium_1.SetFloat(obj_f.speed_walk, 1.9514365f);
            }

            if (earth_elemental_medium_2 != null)
            {
                earth_elemental_medium_2.SetFloat(obj_f.speed_run, 1.9514365f);
                earth_elemental_medium_2.SetFloat(obj_f.speed_walk, 1.9514365f);
            }

            rnte(new locXY(483, 423), 14296, 686, 492, 393, 5.58f, 2, "big", new List<Standpoint>(), out var earth_elemental_large);
            if (earth_elemental_large != null)
            {
                earth_elemental_large.SetFloat(obj_f.speed_run, 1.9514365f);
                earth_elemental_large.SetFloat(obj_f.speed_walk, 1.9514365f);
            }

            // earth_elemental_large.unconceal()  # to prevent lag
            rnte(new locXY(471, 537), 14248, 687, 467, 385, 4.8f, 50, "special", new List<Standpoint>(), out _); // Ogrechief
            rnte(new locXY(445, 444), 8045, 688, 482, 398, 5.5f, 50, "special", new List<Standpoint>(), out _); // Romag
            rnte(new locXY(445, 444), 14154, 689, 470, 403, 5, 50, "special", new List<Standpoint>(), out _); // Hartsch
            rnte(new locXY(505, 534), 14066, 690, 496, 402, 0.4f, 50, "special", new List<Standpoint>(), out _); // Gnoll leader
            rnte(new locXY(442, 458), 14339, 691, 470, 390, 5.1f, 5, "lieutenant", new List<Standpoint>(), out _); // Earth lieutenant
            rnte(new locXY(439, 492), 14338, 692, 485, 387, 5.4f, 1, "melee", reserve_melee, out _); // Earth fighter
            rnte(new locXY(441, 490), 14338, 693, 479, 385, 1, 1, "melee", reserve_melee, out _); // Earth fighter
            rnte(new locXY(444, 494), 14338, 694, 481, 391, 4.9f, 1, "melee", reserve_melee, out _); // Earth fighter
            // if earth_fighter1 == OBJ_HANDLE_NULL:
            // earth_bugbear_4 = rnte(  508 , 477 , 14162 , 692 , 485 , 387 , 5.4  , 1)
            // elif earth_fighter1.stat_level_get(stat_hp_current) < 0:
            // earth_bugbear_4 = rnte(  508 , 477 , 14162 , 692 , 485 , 387 , 5.4  , 1)
            // if earth_fighter2 == OBJ_HANDLE_NULL:
            // earth_bugbear_5 = rnte(  499 , 476 , 14165 , 693 , 479 , 385 , 1    , 1)
            // elif earth_fighter2.stat_level_get(stat_hp_current) < 0:
            // earth_bugbear_5 = rnte(  499 , 476 , 14165 , 693 , 479 , 385 , 1    , 1)
            // if earth_fighter3 == OBJ_HANDLE_NULL:
            // earth_bugbear_6 = rnte(  449 , 449 , 14163 , 694 , 481 , 391 , 4.9  , 1)
            // elif earth_fighter3.stat_level_get(stat_hp_current) < 0:
            // earth_bugbear_6 = rnte(  449 , 449 , 14163 , 694 , 481 , 391 , 4.9  , 1)
            // earth_robe_guard4 = rnte(       445 , 444 , 14154 , 644 , 480 , 419 , 5.4  , 50)
            // earth_archer4 = rnte(           445 , 444 , 14154 , 644 , 480 , 419 , 5.4  , 50)
            // earth_archer5 = rnte(           445 , 444 , 14154 , 644 , 480 , 419 , 5.4  , 50)
            var romagchest = rct(445, 444, 1011, 488, 429, 4);
            var barr1 = GameSystems.MapObject.CreateObject(121, new locXY(469, 379));
            barr1.Rotation = 0.8f;
            barr1.Move(new locXY(469, 379), 10, 0);
            barr1.SetPortalFlag(PortalFlag.JAMMED);
            var barr2 = GameSystems.MapObject.CreateObject(121, new locXY(467, 379));
            barr2.Rotation = 0.8f;
            barr2.Move(new locXY(467, 379), 20, 0.69f);
            barr2.SetPortalFlag(PortalFlag.JAMMED);
            var barr2_npc = GameSystems.MapObject.CreateObject(14914, new locXY(467, 379));
            barr2_npc.Rotation = 0.8f;
            barr2_npc.Move(new locXY(467, 379), 20, 1);
            barr2_npc.FadeTo(0, 0, 255);
            barr2_npc.SetScriptId(ObjScriptEvent.Heartbeat, 446); // heartbeat to bulletproof against PCs walking through the barrier
            var barr3 = GameSystems.MapObject.CreateObject(121, new locXY(466, 379));
            barr3.Rotation = 0.8f;
            barr3.Move(new locXY(466, 379), 0, 1.46f);
            barr3.SetPortalFlag(PortalFlag.JAMMED);
            var barr4 = GameSystems.MapObject.CreateObject(121, new locXY(501, 378));
            barr4.Rotation = 0.8f;
            barr4.Move(new locXY(501, 378), 39, 25);
            barr4.SetPortalFlag(PortalFlag.JAMMED);
            var barr5 = GameSystems.MapObject.CreateObject(121, new locXY(499, 378));
            barr5.Rotation = 0.8f;
            barr5.Move(new locXY(499, 378), 36, 25.898f);
            barr5.SetPortalFlag(PortalFlag.JAMMED);
            var barr6 = GameSystems.MapObject.CreateObject(121, new locXY(497, 378));
            barr6.Rotation = 0.65f;
            barr6.Move(new locXY(497, 378), 32, 23.2f);
            barr6.SetPortalFlag(PortalFlag.JAMMED);
            barr6.SetScriptId(ObjScriptEvent.Heartbeat, 446); // heartbeat to bulletproof against PCs walking through the barrier
            var barr6_npc = GameSystems.MapObject.CreateObject(14914, new locXY(497, 378));
            barr6_npc.Rotation = 0.65f;
            barr6_npc.Move(new locXY(497, 378), 32, 23.4f);
            barr6_npc.FadeTo(0, 0, 255);
            var barr7 = GameSystems.MapObject.CreateObject(121, new locXY(495, 378));
            barr7.Rotation = 0.45f;
            barr7.Move(new locXY(495, 378), 29, 9.9f);
            barr7.SetPortalFlag(PortalFlag.JAMMED);
            return;
        }

        public static void air_reg()
        {
            var kelno = rnt(545, 497, 8092, 700, 480, 494, 1.5f, 5);
            // The kitchen dwellers
            var k1 = rnt(560, 468, 14161, 721, 501, 504, 4.2f, 2);
            var k2 = rnt(553, 469, 14216, 722, 477, 509, 2.3f, 2);
            var k3 = rnt(559, 465, 14159, 723, 495, 497, 4.1f, 2);
            var k4 = rnt(551, 474, 14184, 724, 470, 508, 3, 2);
            if (k4 == null)
            {
                k4 = rnt(561, 470, 14184, 724, 470, 508, 3, 2);
            }

            var k5 = rnt(559, 464, 14185, 725, 497, 507, 3, 2);
            if (k5 == null)
            {
                k5 = rnt(564, 472, 14185, 725, 497, 507, 3, 2);
            }

            var k6 = rnt(553, 480, 14216, 726, 485, 516, 2.35f, 2); // greeter, SE door, female bugbear
            if (k6 == null)
            {
                k6 = rnt(564, 469, 14216, 726, 485, 516, 2.35f, 2);
            }

            if (k6 != null)
            {
                k6.SetScriptId(ObjScriptEvent.EnterCombat, 445);
            }

            // k6.scripts[19] = 445
            var k7 = rnt(552, 478, 14159, 727, 499, 494, 4, 2); // greeter, SW door, bugbear
            if (k7 != null)
            {
                k7.SetScriptId(ObjScriptEvent.EnterCombat, 445);
            }

            // k7.scripts[19] = 445
            var k8 = rnt(561, 479, 14079, 728, 495, 490, 3.7f, 2);
            var k9 = rnt(561, 474, 14079, 729, 502, 513, 4, 2);
            var k10 = rnt(558, 477, 14080, 730, 497, 511, 1, 2);
            var k11 = rnt(564, 474, 14080, 731, 474, 502, 1, 2); // greeter, NE door, gnoll
            if (k11 != null)
            {
                k11.SetScriptId(ObjScriptEvent.EnterCombat, 445);
            }

            // k11.scripts[19] = 445
            var k12 = rnt(564, 480, 14187, 732, 496, 513, 1, 2);
            if (k12 == null)
            {
                k12 = rnt(557, 468, 14187, 732, 496, 513, 1, 2);
            }

            var k13 = rnt(558, 474, 14067, 733, 496, 515, 1, 2);
            var k14 = rnt(563, 477, 14067, 734, 503, 510, 4.5f, 2);
            var k15 = rnt(558, 480, 14067, 735, 496, 485, 4, 2);
            // k16 = rnt(445, 444, 14185, 736, 482, 417, 5.5, 2)
            // The 12 bugbears just outside Kelno's office
            if (k6 != null)
            {
                var g1 = rnt(564, 492, 14159, 701, 477, 496, 1.3f, 2);
            }
            else
            {
                var g1 = rnt(564, 492, 14159, 726, 485, 516, 2.35f, 2); // SE sentry
                if (g1 != null)
                {
                    g1.SetScriptId(ObjScriptEvent.EnterCombat, 445);
                }

            }

            // g1.scripts[19] = 445
            var g2 = rnt(556, 499, 14158, 702, 475, 506, 2, 2);
            var g3 = rnt(570, 489, 14160, 703, 482, 500, 2, 2);
            var g4 = rnt(570, 501, 14160, 704, 484, 498, 3, 2);
            var g5 = rnt(568, 498, 14161, 705, 475, 497, 2, 2);
            var g6 = rnt(565, 499, 14158, 706, 472, 499, 1.5f, 2);
            var g7 = rnt(558, 501, 14215, 707, 474, 489, 3, 2);
            var g8 = rnt(566, 501, 14216, 708, 481, 487, 2, 2);
            if (g8 == null)
            {
                g8 = rnt(563, 501, 14216, 708, 481, 487, 2, 2);
            }

            if (k7 != null)
            {
                var g9 = rnt(567, 494, 14161, 709, 488, 496, 4, 2);
                if (g9 == null)
                {
                    g9 = rnt(560, 500, 14161, 709, 488, 496, 4, 2);
                }

            }
            else
            {
                var g9 = rnt(567, 494, 14161, 727, 499, 494, 4, 2); // SW sentry
                if (g9 == null)
                {
                    g9 = rnt(560, 500, 14161, 727, 499, 494, 4, 2);
                }

                if (g9 != null)
                {
                    g9.SetScriptId(ObjScriptEvent.EnterCombat, 445);
                }

            }

            // g9.scripts[19] = 445
            var g10 = rnt(567, 490, 14159, 710, 491, 493, 4, 2);
            var g11 = rnt(568, 492, 14161, 711, 484, 493, 3, 2);
            var g12 = rnt(562, 490, 14161, 712, 487, 488, 4, 2);
            // Bugbear defectors
            if (GetGlobalFlag(108) || PartyLeader.GetStat(Stat.constitution) >= 50)
            {
                var d1 = rnt(566, 519, 14247, 713, 486, 495, 4.5f, 2);
                var d2 = rnt(559, 520, 14247, 714, 488, 492, 3.5f, 2);
                var d3 = rnt(561, 512, 14247, 715, 475, 492, 1.9f, 2);
                var d4 = rnt(556, 514, 14247, 716, 480, 516, 2.4f, 2);
                var d5_leader = rnt(549, 512, 14231, 717, 492, 486, 3.6f, 5);
            }

            // Air Elementals
            var ae1 = rnt(498, 504, 14380, 718, 478, 494, 5.5f, 3);
            var ae2 = rnt(477, 504, 14380, 719, 483, 490, 5.5f, 3);
            var ae3_large = rnt(486, 493, 14292, 720, 495, 504, 5.5f, 50);
            var kelnochest = rct(541, 495, 1014, 475, 487, 2.9f);
            foreach (var obj1 in ObjList.ListVicinity(new locXY(518, 485), ObjectListFilter.OLC_PORTAL))
            {
                var (x1, y1) = obj1.GetLocation();
                if (x1 == 518 && y1 == 485)
                {
                    obj1.SetSecretDoorFlags(0);
                    if (((obj1.GetPortalFlags() & PortalFlag.LOCKED)) != 0)
                    {
                        obj1.ClearPortalFlag(PortalFlag.LOCKED);
                    }

                }

            }

            // if not ( obj1.portal_flags_get() & OPF_OPEN ):
            // obj1.portal_toggle_open()
            // # obj1.portal_flag_set(OPF_OPEN) - didn't work
            return;
        }
        public static void water_reg()
        {
            var belsornig = rnt(545, 538, 8091, 750, 550, 583, 0, 5);
            var jugg_old = fsnc(545, 538, 1618, 20);
            jugg_old.SetObjectFlag(ObjectFlag.DONTDRAW);
            var juggernaut = GameSystems.MapObject.CreateObject(2110, new locXY(539, 573));
            juggernaut.Move(new locXY(539, 573), 0, 0);
            juggernaut.Rotation = 5.71f;
            // juggernaut = rst(545, 538, 1618, 539, 573, 5.5, 20)
            var gar1 = rnt(557, 533, 14239, 752, 532, 566, 4, 3);
            var gar2 = rnt(556, 548, 14239, 753, 536, 561, 2.5f, 3);
            var gar3 = rnt(522, 547, 14239, 754, 545, 562, 2.5f, 3);
            var gar4 = rnt(523, 534, 14239, 755, 547, 565, 1, 3);
            var bug1 = rnt(553, 547, 14181, 757, 539, 564, 5.5f, 3);
            if (bug1 != null)
            {
                bug1.SetScriptId(ObjScriptEvent.EnterCombat, 445); // enter combat
                bug1.SetScriptId(ObjScriptEvent.Heartbeat, 445); // heartbeat
                var bug2 = rnt(554, 533, 14181, 756, 531, 573, 5, 3);
            }
            else
            {
                var bug2 = rnt(554, 533, 14181, 757, 539, 564, 5.5f, 3);
                if (bug2 != null)
                {
                    bug2.SetScriptId(ObjScriptEvent.EnterCombat, 445);
                    bug1.SetScriptId(ObjScriptEvent.Heartbeat, 445);
                }

            }

            var nalorrem = rnt(548, 543, 8028, 758, 546, 586, 5.5f, 5);
            var merrolan = rnt(548, 535, 8027, 759, 533, 584, 5, 5);
            if (!GetGlobalFlag(108))
            {
                var defec1_leader = rnt(549, 512, 14231, 760, 547, 577, 0, 5);
                var defec2 = rnt(566, 519, 14247, 761, 526, 581, 5, 2);
                var defec3 = rnt(559, 520, 14247, 762, 531, 578, 5, 2);
                var defec4 = rnt(561, 512, 14247, 763, 534, 577, 5.5f, 2);
                var defec5 = rnt(556, 514, 14247, 764, 537, 580, 6, 2);
            }

            var owlbear = rnt(512, 557, 14046, 765, 533, 568, 5, 5);
            var kelleen = rnt(509, 562, 14225, 766, 531, 570, 5, 5);
            if (GetGlobalFlag(112) && !GetGlobalFlag(105)) // convinced oohlgrist via dialogue, and Alrrem is not dead (failsafe)
            {
                var oohlgrist = rnt(483, 614, 14195, 767, 546, 568, 0, 5);
                var troll1 = rnt(496, 619, 14262, 768, 544, 567, 1, 5);
                var troll2 = rnt(473, 610, 14262, 769, 544, 571, 0.5f, 5);
            }

            var snake1 = rnt(531, 589, 14375, 770, 532, 587, 4.6f, 5);
            var snake2 = rnt(535, 598, 14375, 771, 528, 584, -1.6f, 5);
            var snake3 = rnt(543, 599, 14375, 772, 553, 580, 0.5f, 5);
            var snake4 = rnt(549, 588, 14375, 773, 547, 583, 0, 5);
            var belsornigchest = rct(551, 566, 1013, 554, 587, 5);
            return;
        }
        public static void fire_reg()
        {
            var fire_bugbear_1 = rnt(403, 474, 14167, 870, 418, 487, 1, 1);
            var fire_bugbear_2 = rnt(408, 474, 14167, 871, 411, 511, 4, 1);
            var fire_bugbear_3 = rnt(416, 472, 14168, 872, 424, 498, 1, 1);
            var fire_bugbear_4 = rnt(419, 473, 14169, 873, 415, 488, 5, 1);
            var fire_bugbear_5 = rnt(398, 488, 14169, 874, 415, 499, 2.5f, 1);
            var fire_bugbear_6 = rnt(421, 515, 14169, 875, 420, 510, 1, 1);
            var fire_bugbear_7 = rnt(415, 515, 14169, 876, 420, 501, 2, 1);
            // fire_bugbear_8 = rnt(          403 , 474 , 14169 , 877 , 418 , 487 , 1  , 1)
            // fire_bugbear_9 = rnt(          403 , 474 , 14169 , 878 , 418 , 487 , 1  , 1)
            var alrrem = rnt(426, 490, 8047, 879, 419, 494, 1, 1);
            var fire_hydra = rnt(463, 557, 14343, 880, 416, 507, 2.5f, 1);
            var aern = rnt(461, 563, 14224, 881, 414, 503, 2, 1);
            // if game.global_flags[118] == 1  or game.party[0].stat_level_get(stat_constitution) >= 50: # enable for testing with uber party
            if (GetGlobalFlag(118) && !GetGlobalFlag(107)) // convinced oohlgrist via dialogue, and Alrrem is not dead (failsafe)
            {
                var oohlgrist = rnt(483, 614, 14195, 882, 413, 509, 3, 5);
                var troll1 = rnt(496, 619, 14262, 883, 413, 483, 4, 5);
                var troll2 = rnt(473, 610, 14262, 884, 414, 492, 3.5f, 5);
            }

            var werewolf1 = rnt(440, 471, 14344, 885, 410, 483, 2.5f, 1);
            var werewolf2 = rnt(439, 468, 14344, 886, 421, 507, 0, 1);
            // the two below are due to disparities between standpoints and initial locations as indicated by MOB file: (wouldn't hurt to fix the MOB)
            if (fire_bugbear_4 == null)
            {
                fire_bugbear_4 = rnt(414, 494, 14169, 873, 415, 488, 5, 1);
            }

            if (fire_bugbear_5 == null)
            {
                fire_bugbear_5 = rnt(412, 498, 14169, 874, 415, 499, 2.5f, 1);
            }

            return;
        }

        public static GameObjectBody rnt(int sourceX, int sourceY, int obj_name, int new_standpoint_ID, int new_x, int new_y, float new_rotation, int radius)
        {
            // Relocate NPC To...
            // source_x, source_y - where the object currently is
            // obj_name - self explanatory...
            // new_standpoint_ID - assign new standpoint from the jumppoint.tab file, so the NPC doesn't try to get back to its old location
            // new_x, new_y, new_rotation - where the object is transferred to, and which rotation it is given
            // radius - this is used to limit the range of detection of the critter, in case you want to transfer a specific one
            // (i.e. if there are two Earth Temple troops close together but you want to pick a particular one, then use a small radius)
            var transferee = fnnc(new locXY(sourceX, sourceY), obj_name, radius);
            if (transferee != null)
            {
                if (!Utilities.critter_is_unconscious(transferee))
                {
                    sps(transferee, new_standpoint_ID);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    transferee.Move(new locXY((int) new_x, (int) new_y), 0, 0);
                    transferee.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                    transferee.Rotation = new_rotation;
                }

            }

            return transferee;
        }
        private static void rnte(locXY sourceLocation,
            int obj_name,
            int new_standpoint_ID,
            int new_x, int new_y,
            float new_rotation,
            int radius,
            string extra_command,
            List<Standpoint> reserve_list,
            out GameObjectBody transferee)
        {
            // Relocate NPC To... EARTH TEMPLE VARIANT
            // source_x, source_y - where the object currently is
            // obj_name - self explanatory...
            // new_standpoint_ID - assign new standpoint from the jumppoint.tab file, so the NPC doesn't try to get back to its old location
            // new_x, new_y, new_rotation - where the object is transferred to, and which rotation it is given
            // radius - this is used to limit the range of detection of the critter, in case you want to transfer a specific one
            // (i.e. if there are two Earth Temple troops close together but you want to pick a particular one, then use a small radius)
            // If the 'source NPC' is found, and it is conscious, it will be transferred.
            // Else, the reserve list is used.
            transferee = fnnc(sourceLocation, obj_name, radius);
            if (transferee != null)
            {
                if (!Utilities.critter_is_unconscious(transferee)) // NB: OBJ_HANDLE_NULL can't be checked for unconsciousness, it would fuck up the script
                {
                    if (extra_command == "lieutenant")
                    {
                        transferee.SetInt(obj_f.critter_strategy, 0);
                    }

                    sps(transferee, new_standpoint_ID);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    transferee.Move(new locXY(new_x, new_y), 0, 0);
                    transferee.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                    transferee.Rotation = new_rotation;
                    transferee.SetInt(obj_f.npc_pad_i_4, new_standpoint_ID);
                    if (extra_command == "melee" || extra_command == "big")
                    {
                        transferee.SetScriptId(ObjScriptEvent.EnterCombat, 446); // Enter Combat
                        transferee.SetScriptId(ObjScriptEvent.ExitCombat, 446); // Exit Combat
                        transferee.SetScriptId(ObjScriptEvent.StartCombat, 446); // Start Combat (round)
                    }

                }
                else if ((extra_command == "melee" || extra_command == "ranged") && reserve_list.Count > 0)
                {
                    transferee = fnnc(reserve_list[0]);
                    reserve_list.RemoveAt(0);

                    sps(transferee, new_standpoint_ID);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    transferee.Move(new locXY(new_x, new_y), 0, 0);
                    transferee.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                    transferee.Rotation = new_rotation;
                    transferee.SetInt(obj_f.npc_pad_i_4, new_standpoint_ID);
                    if (extra_command == "melee")
                    {
                        transferee.SetScriptId(ObjScriptEvent.EnterCombat, 446); // Enter Combat
                        transferee.SetScriptId(ObjScriptEvent.ExitCombat, 446); // Exit Combat
                        transferee.SetScriptId(ObjScriptEvent.StartCombat, 446); // Start Combat (round)
                    }

                }

            }
            else if ((extra_command == "melee" || extra_command == "ranged") && reserve_list.Count > 0) // I assume invalids have already been trimmed from the list
            {
                transferee = fnnc(reserve_list[0]);
                reserve_list.RemoveAt(0);

                sps(transferee, new_standpoint_ID);
                transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                transferee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                transferee.Move(new locXY(new_x, new_y), 0, 0);
                transferee.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                transferee.Rotation = new_rotation;
                transferee.SetInt(obj_f.npc_pad_i_4, new_standpoint_ID);
                if (extra_command == "melee")
                {
                    transferee.SetScriptId(ObjScriptEvent.EnterCombat, 446); // Enter Combat
                    transferee.SetScriptId(ObjScriptEvent.ExitCombat, 446); // Exit Combat
                    transferee.SetScriptId(ObjScriptEvent.StartCombat, 446); // Start Combat (round)
                }

            }
        }

        private static List<Standpoint> trim_dead(IEnumerable<Standpoint> untrimmed_list)
        {
            var result = new List<Standpoint>();
            foreach (var pp in untrimmed_list)
            {
                var candidate = fnnc(pp.Location, pp.NameId, pp.Radius);
                if (candidate != null)
                {
                    if (!Utilities.critter_is_unconscious(candidate))
                    {
                        result.Add(pp);
                    }

                }
            }
            return result;
        }
        public static GameObjectBody rct(int script_x, int script_y, int obj_name, int new_x, int new_y, float new_rotation)
        {
            // Relocate Container To...
            var transferee = fcnc(script_x, script_y, obj_name);
            if (transferee != null)
            {
                transferee.Move(new locXY(new_x, new_y), 0, 0);
                transferee.Rotation = new_rotation;
            }

            return transferee;
        }
        public static GameObjectBody rst(int script_x, int script_y, int obj_name, int new_x, int new_y, float new_rotation, int radius)
        {
            // Relocate Scenery To...
            var transferee = fsnc(script_x, script_y, obj_name, radius);
            if (transferee != null)
            {
                transferee.Move(new locXY(new_x, new_y), 0, 0);
                transferee.Rotation = new_rotation;
            }

            return transferee;
        }
        public static void sps(GameObjectBody object_to_be_transferred, int new_standpoint_ID)
        {
            // standpoint set
            object_to_be_transferred.SetStandpoint(StandPointType.Day, new_standpoint_ID);
            object_to_be_transferred.SetStandpoint(StandPointType.Night, new_standpoint_ID);
            return;
        }

        private static GameObjectBody fnnc(Standpoint standpoint) =>
            fnnc(standpoint.Location, standpoint.NameId, standpoint.Radius);

        private static GameObjectBody fnnc(locXY location, int name, int radius = 1)
        {
            // Find NPC near coordinate, detection radius optional
            foreach (var npc in ObjList.ListVicinity(location, ObjectListFilter.OLC_NPC))
            {
                var dist = Vector2.Distance(location.ToInches2D(), npc.GetLocation().ToInches2D());
                if ((npc.GetNameId() == name && dist <= radius))
                {
                    return npc;
                }

            }

            return null;
        }
        public static GameObjectBody fcnc(int xx, int yy, int name)
        {
            // Find container near coordinate
            foreach (var container in ObjList.ListVicinity(new locXY(xx, yy), ObjectListFilter.OLC_CONTAINER))
            {
                if ((container.GetNameId() == name))
                {
                    return container;
                }

            }

            return null;
        }
        public static GameObjectBody fsnc(int xx, int yy, int name, int radius)
        {
            // Find scenery near coordinate
            foreach (var mang in ObjList.ListVicinity(new locXY(xx, yy), ObjectListFilter.OLC_SCENERY))
            {
                var (mang_x, mang_y) = mang.GetLocation();
                var dist = MathF.Sqrt((mang_x - xx) * (mang_x - xx) + (mang_y - yy) * (mang_y - yy));
                if ((mang.GetNameId() == name && dist <= radius))
                {
                    return mang;
                }

            }

            return null;
        }
        public static List<GameObjectBody> vlist()
        {
            using var moshe = ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC);
            return new List<GameObjectBody>(moshe);
        }

        public static GameObjectBody spawn(int prot, int x, int y)
        {
            var moshe = GameSystems.MapObject.CreateObject(prot, new locXY(x, y));
            if ((moshe != null))
            {
                return moshe;
            }

            return null;
        }

    }
}
