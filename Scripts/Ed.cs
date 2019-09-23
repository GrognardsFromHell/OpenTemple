
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
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class Ed
    {
        // Editor's Helper Script version 6.0 - (C) 2005-2006 by Agetian
        // Batch mod testing commands (C) 2005 by Cerulean the Blue
        // Sector reader/writer routines are portions of ToEEWB foreported to Python 2.x
        // (C) copyright 2005 by Michael "Agetian" Kamensky
        // ==========================================================================
        // PLEASE MODIFY TO SUIT YOUR NEEDS:
        // PATH_TO_SECTORS - Default path to sector files (where to load and save)
        // PATH_TO_INTEROP - Default path to the synchronizer (must be a same fol-
        // der as specified in the ToEE World Builder 2.0.0)
        // Note that you need a double slash as a path separator
        // here!

        private static readonly string PATH_TO_SECTORS = "C:\\Sectors";
        private static readonly string PATH_TO_INTEROP = "C:\\";
        // ==========================================================================
        // Critical constants. Do NOT modify!

        private static readonly int TILE_IMPASSABLE = "\u0002" + "\0" * 4 + "þ\u0003" + "\0" * 9;
        private static readonly int TILE_FLYOVER = "\u0002" + "\0" * 5 + "ü\a" + "\0" * 8;
        private static readonly int TILE_FLYOVER_COVER = "\u0002" + "\0" * 5 + "ü\u000f" + "\0" * 8;
        private static readonly int TILE_WATER = "$" * 9;
        private static readonly int TILE_NO_WATER = "\0" * 9;
        public static string secloc()
        {
            var secID = _secloc(PartyLeader.GetLocation());
            return secID[0].ToString() + ".sec  (SectorX=" + secID[1].ToString() + ", SectorY=" + secID[2].ToString() + ")";
        }
        public static FIXME _secloc(FIXME coords_tuple)
        {
            var (sec_X, sec_Y) = (coords_tuple[0] / 64, coords_tuple[1] / 64);
            return new[] { ((sec_Y * 4) << 24) + sec_X, sec_X, sec_Y };
        }
        public static void _createemptysector(FIXME sector_file)
        {
            var f = open(sector_file, "wb");
            f.write/*Unknown*/("\0\0\0\0");
            for (var i = 0; i < 4096; i++)
            {
                f.write/*Unknown*/("\u0002\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0");
            }

            f.write/*Unknown*/("\u0001\0\0\0\u0004\0ª");
            for (var j = 0; j < 45; j++)
            {
                f.write/*Unknown*/("\0");
            }

            f.close/*Unknown*/();
        }
        public static FIXME _world2sector(FIXME coords_tuple)
        {
            var (sec_X, sec_Y) = (coords_tuple[0] / 64, coords_tuple[1] / 64);
            return ((sec_Y * 4) << 24) + sec_X;
        }
        public static FIXME _world2sec_coords(FIXME coords_tuple)
        {
            var (sec_X, sec_Y) = (coords_tuple[0] / 64, coords_tuple[1] / 64);
            return (sec_X, sec_Y);
        }
        public static int _sec_getminXY(FIXME sector_coords_tuple)
        {
            return (sector_coords_tuple[0] * 64, sector_coords_tuple[1] * 64);
        }
        public static int _sec_getmaxXY(FIXME sector_coords_tuple)
        {
            return (sector_coords_tuple[0] * 64 + 63, sector_coords_tuple[1] * 64 + 63);
        }
        public static FIXME _sec_getXY(FIXME object_coords)
        {
            var (min_X, min_Y) = _sec_getminXY(_world2sec_coords(object_coords));
            var (obj_X, obj_Y) = (object_coords[0] - min_X, object_coords[1] - min_Y);
            return (obj_X, obj_Y);
        }
        public static int _sec_gettilepos(FIXME object_coords)
        {
            var (pos_X, pos_Y) = _sec_getXY(object_coords);
            return (pos_Y * 64 + pos_X) * 16;
        }
        public static void _sec_writetile(FIXME file, FIXME tile_data_string, FIXME tile_pos)
        {
            try
            {
                var f = open(file, "rb");
            }
            catch (Exception e)
            {
                _createemptysector(file);
                f = open(file, "rb");
            }

            var data = f.read/*Unknown*/();
            f.close/*Unknown*/();
            var tile_idx = data.index/*Unknown*/("\u0001\0\0\0\u0004\0ª\0\0\0\0") - 65536;
            tile_idx += tile_pos;
            data = data[..tile_idx] + tile_data_string + data[tile_idx + 16..];
            f = open(file, "wb");
            f.write/*Unknown*/(data);
            f.close/*Unknown*/();
        }
        public static string _sec_getfile()
        {
            return PATH_TO_SECTORS + "\\" + _world2sector(PartyLeader.GetLocation()).ToString() + ".sec";
        }
        public static void _createemptyhsd(FIXME hsd_file)
        {
            var f = open(hsd_file, "wb");
            f.write/*Unknown*/("\u0002\0\0\0");
            for (var x = 0; x < 4096; x++)
            {
                f.write/*Unknown*/("\0" * 9);
            }

            f.close/*Unknown*/();
        }
        public static int _hsd_getpos(FIXME object_coords)
        {
            var (pos_X, pos_Y) = _sec_getXY(object_coords);
            return 4 + ((pos_Y * 64 + pos_X) * 9);
        }
        public static void _hsd_writetile(FIXME file, FIXME hsd_data_string, FIXME tile_pos)
        {
            try
            {
                var f = open(file, "rb");
            }
            catch (Exception e)
            {
                _createemptyhsd(file);
                f = open(file, "rb");
            }

            var data = f.read/*Unknown*/();
            f.close/*Unknown*/();
            data = data[..tile_pos] + hsd_data_string + data[tile_pos + 9..];
            f = open(file, "wb");
            f.write/*Unknown*/(data);
            f.close/*Unknown*/();
        }
        public static string _hsd_getfile()
        {
            return PATH_TO_SECTORS + "\\hsd" + _world2sector(PartyLeader.GetLocation()).ToString() + ".hsd";
        }
        public static void _createemptysvb(FIXME svb_file)
        {
            var f = open(svb_file, "wb");
            f.write/*Unknown*/("\0" * 18432);
            f.close/*Unknown*/();
        }
        public static string _svb_getfile()
        {
            return PATH_TO_SECTORS + "\\" + _world2sector(PartyLeader.GetLocation()).ToString() + ".svb";
        }
        public static string _svb_getfile_plus11()
        {
            var (x, y) = PartyLeader.GetLocation();
            x += 11;
            return PATH_TO_SECTORS + "\\" + _world2sector((x, y)).ToString() + ".svb";
        }
        public static string _svb_getfile_plus22()
        {
            var (x, y) = PartyLeader.GetLocation();
            x += 22;
            return PATH_TO_SECTORS + "\\" + _world2sector((x, y)).ToString() + ".svb";
        }
        public static string _svb_getfile_minus11()
        {
            var (x, y) = PartyLeader.GetLocation();
            x -= 11;
            return PATH_TO_SECTORS + "\\" + _world2sector((x, y)).ToString() + ".svb";
        }
        public static int _svb_getpos(FIXME object_coords)
        {
            var (pos_X, pos_Y) = _sec_getXY(object_coords);
            return (int)((pos_Y * 64 + pos_X) * 4.5f);
        }
        public static void _svb_writetile(FIXME file, FIXME pos_X, FIXME pos_Y)
        {
            try
            {
                var f = open(file, "rb");
            }
            catch (Exception e)
            {
                _createemptysvb(file);
                f = open(file, "rb");
            }

            var data = f.read/*Unknown*/();
            f.close/*Unknown*/();
            // CHECK THE TILE DATA
            var tloc = _svb_getpos((pos_X, pos_Y));
            var type = 0;
            if (pos_X % 2 == 0)
            {
                // second
                var check_loc = ord(data[tloc + 4]);
                // print check_loc
                type = 1;
            }
            else
            {
                // first
                var check_loc = ord(data[tloc]);
                // print check_loc
                type = 2;
            }

            var EXTRA_BYTE = 0;
            if (type == 1) // first
            {
                // print 'first'
                if (check_loc == 0)
                {
                    EXTRA_BYTE = 1;
                }
                else if (check_loc == 1)
                {
                    EXTRA_BYTE = 1;
                }
                else if (check_loc == 17)
                {
                    EXTRA_BYTE = 17;
                }
                else if (check_loc == 16)
                {
                    EXTRA_BYTE = 17;
                }

                var strdef = "\u0011" * 4 + chr(EXTRA_BYTE);
            }
            else
            {
                // print repr(strdef)
                // else: # second
                if (check_loc == 0)
                {
                    EXTRA_BYTE = 16;
                }
                else if (check_loc == 1)
                {
                    EXTRA_BYTE = 17;
                }
                else if (check_loc == 16)
                {
                    EXTRA_BYTE = 17;
                }
                else if (check_loc == 17)
                {
                    EXTRA_BYTE = 17;
                }

                var strdef = chr(EXTRA_BYTE) + "\u0011" * 4;
            }

            // print repr(strdef)
            data = data[..tloc] + strdef + data[tloc + 5..];
            f = open(file, "wb");
            f.write/*Unknown*/(data);
            f.close/*Unknown*/();
        }
        public static void wtr()
        {
            _hsd_writetile(_hsd_getfile(), TILE_WATER, _hsd_getpos(PartyLeader.GetLocation()));
            Logger.Info("{0}", (PartyLeader.GetLocation().ToString() + " was marked WATER"));
            return;
        }
        public static void lnd()
        {
            _hsd_writetile(_hsd_getfile(), TILE_NO_WATER, _hsd_getpos(PartyLeader.GetLocation()));
            Logger.Info("{0}", (PartyLeader.GetLocation().ToString() + " was marked NON-WATER (LAND)"));
            return;
        }
        public static void blk()
        {
            _sec_writetile(_sec_getfile(), TILE_IMPASSABLE, _sec_gettilepos(PartyLeader.GetLocation()));
            Logger.Info("{0}", (PartyLeader.GetLocation().ToString() + " was marked IMPASSABLE"));
            return;
        }
        public static void fly()
        {
            _sec_writetile(_sec_getfile(), TILE_FLYOVER, _sec_gettilepos(PartyLeader.GetLocation()));
            Logger.Info("{0}", (PartyLeader.GetLocation().ToString() + " was marked IMPASSABLE, CAN FLY OVER"));
            return;
        }
        public static void cov()
        {
            _sec_writetile(_sec_getfile(), TILE_FLYOVER_COVER, _sec_gettilepos(PartyLeader.GetLocation()));
            Logger.Info("{0}", (PartyLeader.GetLocation().ToString() + " was marked IMPASSABLE, CAN FLY OVER, PROVIDES COVER"));
            return;
        }
        public static bool tp(FIXME map, FIXME X, FIXME Y)
        {
            FadeAndTeleport(0, 0, 0, map, X, Y);
            return RunDefault;
        }
        public static bool tpl(FIXME X, FIXME Y)
        {
            FadeAndTeleport(0, 0, 0, PartyLeader.GetMap(), X, Y);
            return RunDefault;
        }
        public static FIXME loc()
        {
            return PartyLeader.GetLocation();
        }
        public static List<GameObjectBody> objs()
        {
            return ObjList.ListVicinity(PartyLeader.GetLocation(), ObjectListFilter.OLC_ALL);
        }
        public static FIXME locx(FIXME array, FIXME index)
        {
            return array[index].location/*Unknown*/;
        }
        public static void inc_x()
        {
            var (loc_x, loc_y) = PartyLeader.GetLocation();
            FadeAndTeleport(0, 0, 0, PartyLeader.GetMap(), loc_x + 1, loc_y);
        }
        public static void inc_y()
        {
            var (loc_x, loc_y) = PartyLeader.GetLocation();
            FadeAndTeleport(0, 0, 0, PartyLeader.GetMap(), loc_x, loc_y + 1);
        }
        public static void dec_x()
        {
            var (loc_x, loc_y) = PartyLeader.GetLocation();
            FadeAndTeleport(0, 0, 0, PartyLeader.GetMap(), loc_x - 1, loc_y);
        }
        public static void dec_y()
        {
            var (loc_x, loc_y) = PartyLeader.GetLocation();
            FadeAndTeleport(0, 0, 0, PartyLeader.GetMap(), loc_x, loc_y - 1);
        }
        // + paint IMPASSABLE on-off +

        public static void blk_on()
        {
            if (GetGlobalFlag(1000))
            {
                Logger.Info("{0}", ("ALREADY PAINTING. PLEASE CANCEL THE PREVIOUS PAINT MODE."));
                return;
            }

            SetGlobalFlag(1000, true);
            Logger.Info("{0}", ("Painting impassable tiles. Move your character to paint."));
            _blk_on_core();
        }
        public static void _blk_on_core()
        {
            _sec_writetile(_sec_getfile(), TILE_IMPASSABLE, _sec_gettilepos(PartyLeader.GetLocation()));
            if (GetGlobalFlag(1000))
            {
                StartTimer(10, () => _blk_on_core());
            }

            return;
        }
        public static void blk_off()
        {
            SetGlobalFlag(1000, false);
            Logger.Info("{0}", ("Stopped painting."));
        }
        // - paint IMPASSABLE on-off -
        // + paint FLYOVER on-off +

        public static void fly_on()
        {
            if (GetGlobalFlag(1000))
            {
                Logger.Info("{0}", ("ALREADY PAINTING. PLEASE CANCEL THE PREVIOUS PAINT MODE."));
                return;
            }

            SetGlobalFlag(1000, true);
            Logger.Info("{0}", ("Painting fly-over tiles. Move your character to paint."));
            _fly_on_core();
        }
        public static void _fly_on_core()
        {
            _sec_writetile(_sec_getfile(), TILE_FLYOVER, _sec_gettilepos(PartyLeader.GetLocation()));
            if (GetGlobalFlag(1000))
            {
                StartTimer(10, () => _fly_on_core());
            }

            return;
        }
        public static void fly_off()
        {
            SetGlobalFlag(1000, false);
            Logger.Info("{0}", ("Stopped painting."));
        }
        // - paint FLYOVER on-off -
        // + paint FLYOVER/COVER on-off +

        public static void cov_on()
        {
            if (GetGlobalFlag(1000))
            {
                Logger.Info("{0}", ("ALREADY PAINTING. PLEASE CANCEL THE PREVIOUS PAINT MODE."));
                return;
            }

            SetGlobalFlag(1000, true);
            Logger.Info("{0}", ("Painting fly-over/cover tiles. Move your character to paint."));
            _cov_on_core();
        }
        public static void _cov_on_core()
        {
            _sec_writetile(_sec_getfile(), TILE_FLYOVER_COVER, _sec_gettilepos(PartyLeader.GetLocation()));
            if (GetGlobalFlag(1000))
            {
                StartTimer(10, () => _cov_on_core());
            }

            return;
        }
        public static void cov_off()
        {
            SetGlobalFlag(1000, false);
            Logger.Info("{0}", ("Stopped painting."));
        }
        // - paint FLYOVER/COVER on-off -
        // + paint WATER on-off +

        public static void wtr_on()
        {
            if (GetGlobalFlag(1000))
            {
                Logger.Info("{0}", ("ALREADY PAINTING. PLEASE CANCEL THE PREVIOUS PAINT MODE."));
                return;
            }

            SetGlobalFlag(1000, true);
            Logger.Info("{0}", ("Painting water tiles. Move your character to paint."));
            _wtr_on_core();
        }
        public static void _wtr_on_core()
        {
            _hsd_writetile(_hsd_getfile(), TILE_WATER, _hsd_getpos(PartyLeader.GetLocation()));
            if (GetGlobalFlag(1000))
            {
                StartTimer(10, () => _wtr_on_core());
            }

            return;
        }
        public static void wtr_off()
        {
            SetGlobalFlag(1000, false);
            Logger.Info("{0}", ("Stopped painting."));
        }
        // - paint WATER on-off -
        // + paint LAND on-off +

        public static void lnd_on()
        {
            if (GetGlobalFlag(1000))
            {
                Logger.Info("{0}", ("ALREADY PAINTING. PLEASE CANCEL THE PREVIOUS PAINT MODE."));
                return;
            }

            SetGlobalFlag(1000, true);
            Logger.Info("{0}", ("Painting land (non-water) tiles. Move your character to paint."));
            _lnd_on_core();
        }
        public static void _lnd_on_core()
        {
            _hsd_writetile(_hsd_getfile(), TILE_NO_WATER, _hsd_getpos(PartyLeader.GetLocation()));
            if (GetGlobalFlag(1000))
            {
                StartTimer(10, () => _lnd_on_core());
            }

            return;
        }
        public static void lnd_off()
        {
            SetGlobalFlag(1000, false);
            Logger.Info("{0}", ("Stopped painting."));
        }
        // - paint LAND on-off -
        // + Cerulean's batch commands +
        // CB - sets entire groups experience points to xp

        public static int partyxpset(FIXME xp)
        {
            // def partyxpset( xp ):  # CB - sets entire groups experience points to xp
            var pc = SelectedPartyLeader;
            foreach (var obj in pc.GetPartyMembers())
            {
                var curxp = obj.GetStat(Stat.experience);
                var newxp = curxp + xp;
                obj.SetBaseStat(Stat.experience, newxp);
            }

            return 1;
        }
        // CB - sets entire groups xp to minimum necessary for level imputted

        public static int partylevelset(FIXME level)
        {
            // def partylevelset(level):  # CB - sets entire groups xp to minimum necessary for level imputted
            var pc = SelectedPartyLeader;
            foreach (var obj in pc.GetPartyMembers())
            {
                var newxp = (level * (500 * (level - 1)));
                obj.SetBaseStat(Stat.experience, newxp);
            }

            return 1;
        }
        // CB - sets ability to score for entire group

        public static int partyabset(FIXME ab, FIXME score)
        {
            // def partyabset (ab, score):  # CB - sets ability to score for entire group
            if ((ab == 1))
            {
                var abstat = Stat.strength;
            }
            else if ((ab == 2))
            {
                var abstat = Stat.dexterity;
            }
            else if ((ab == 3))
            {
                var abstat = Stat.constitution;
            }
            else if ((ab == 4))
            {
                var abstat = Stat.intelligence;
            }
            else if ((ab == 5))
            {
                var abstat = Stat.wisdom;
            }
            else if ((ab == 6))
            {
                var abstat = Stat.charisma;
            }
            else
            {
                return 0;
            }

            if (((score > 0) && (score < 41)))
            {
                var pc = SelectedPartyLeader;
                foreach (var obj in pc.GetPartyMembers())
                {
                    obj.SetBaseStat(abstat, score);
                }

            }
            else
            {
                return 0;
            }

            return 1;
        }
        // CB - sets all ability scores of specified pc to score

        public static int massabset(FIXME num, FIXME score)
        {
            // def massabset(num, score):  # CB - sets all ability scores of specified pc to score
            num = num - 1;
            var pc = GameSystems.Party.GetPartyGroupMemberN(num);
            if ((pc != null))
            {
                if (((score > 0) && (score < 41)))
                {
                    pc.SetBaseStat(Stat.strength, score);
                    pc.SetBaseStat(Stat.dexterity, score);
                    pc.SetBaseStat(Stat.constitution, score);
                    pc.SetBaseStat(Stat.intelligence, score);
                    pc.SetBaseStat(Stat.wisdom, score);
                    pc.SetBaseStat(Stat.charisma, score);
                }
                else
                {
                    return 0;
                }

            }
            else
            {
                return 0;
            }

            return 1;
        }
        // CB - sets max hp of entire party to specified value

        public static int partyhpset(FIXME hp)
        {
            // def partyhpset(hp):  # CB - sets max hp of entire party to specified value
            var pc = SelectedPartyLeader;
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.SetBaseStat(Stat.hp_max, hp);
            }

            return 1;
        }
        // - Cerulean's batch commands -
        // + INTEROPERABILITY LAYER FOR TOEEWB 2.X.X SERIES +
        // LOCOBJ - put the current X/Y coords into the WB object editor

        public static void locobj()
        {
            // def locobj():  # LOCOBJ - put the current X/Y coords into the WB object editor
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            f.write/*Unknown*/("OBJLOC " + x.ToString() + " " + y.ToString());
            f.close/*Unknown*/();
        }
        // LOCJMP - put the current X/Y/map coords into the WB jump point editor

        public static void locjmp()
        {
            // def locjmp():  # LOCJMP - put the current X/Y/map coords into the WB jump point editor
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("JMPLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // LOCDAY - put the current X/Y/map coords into the WB day/night transition (day)

        public static void locday()
        {
            // def locday():  # LOCDAY - put the current X/Y/map coords into the WB day/night transition (day)
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("DAYLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // LOCNGT - put the current X/Y/map coords into the WB day/night transition (night)

        public static void locngt()
        {
            // def locngt():  # LOCNGT - put the current X/Y/map coords into the WB day/night transition (night)
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("NGTLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // PND - put the current X/Y coords into the WB pathnode generator

        public static void pnd()
        {
            // def pnd():  # PND - put the current X/Y coords into the WB pathnode generator
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            f.write/*Unknown*/("PNDLOC " + x.ToString() + " " + y.ToString());
            f.close/*Unknown*/();
        }
        // WAY - put the current X/Y coords into the WB object waypoint entry

        public static void way()
        {
            // def way():  # WAY - put the current X/Y coords into the WB object waypoint entry
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            f.write/*Unknown*/("WAYLOC " + x.ToString() + " " + y.ToString());
            f.close/*Unknown*/();
        }
        // LIGHT - put the current X/Y/map coords into the WB light editor

        public static void light()
        {
            // def light():  # LIGHT - put the current X/Y/map coords into the WB light editor
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("LGTLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // STANDDAY - put the current X/Y/map coords into the WB day standpoint

        public static void standday()
        {
            // def standday():  # STANDDAY - put the current X/Y/map coords into the WB day standpoint
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("STDDLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // STANDNIGHT - put the current X/Y/map coords into the WB night standpoint

        public static void standnight()
        {
            // def standnight():  # STANDNIGHT - put the current X/Y/map coords into the WB night standpoint
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("STDNLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // STANDSCOUT - put the current X/Y/map coords into the WB scout standpoint

        public static void standscout()
        {
            // def standscout():  # STANDSCOUT - put the current X/Y/map coords into the WB scout standpoint
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var amap = PartyLeader.GetMap();
            f.write/*Unknown*/("STDSLOC " + x.ToString() + " " + y.ToString() + " " + amap.ToString());
            f.close/*Unknown*/();
        }
        // SETROT - set the game.party[0].rotation

        public static void setrot(FIXME value)
        {
            // def setrot(value): # SETROT - set the game.party[0].rotation
            PartyLeader.Rotation = value;
        }
        // LOCOBJR - return X/Y and the rotation

        public static void locobjr()
        {
            // def locobjr(): # LOCOBJR - return X/Y and the rotation
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var a = PartyLeader.Rotation.ToString()[0..8];
            f.write/*Unknown*/("OBJLOCR " + x.ToString() + " " + y.ToString() + " " + a);
            f.close/*Unknown*/();
        }
        // WAYR - put the current X/Y coords into the WB object waypoint entry with rotation

        public static void wayr()
        {
            // def wayr():  # WAYR - put the current X/Y coords into the WB object waypoint entry with rotation
            var f = open(PATH_TO_INTEROP + "\\wb200_il.lri", "w");
            var (x, y) = loc();
            var a = PartyLeader.Rotation.ToString()[0..8];
            f.write/*Unknown*/("WAYLOCR " + x.ToString() + " " + y.ToString() + " " + a);
            f.close/*Unknown*/();
        }

    }
}
