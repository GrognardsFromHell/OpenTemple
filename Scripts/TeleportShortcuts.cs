
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
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class TeleportShortcuts
    {
        // TELEPORT SHORTCUTS		#

        public static void shopmap()
        {
            FadeAndTeleport(0, 0, 0, 5107, 480, 480);
            return;
        }
        public static void lgvignette()
        {
            FadeAndTeleport(0, 0, 0, 5096, 480, 480);
            return;
        }
        public static void lawfulgoodvignette()
        {
            lgvignette();
            return;
        }
        public static void lgvig()
        {
            lgvignette();
            return;
        }
        // HOMMLET			#

        public static void homm()
        {
            FadeAndTeleport(0, 0, 0, 5001, 619, 422);
            return;
        }
        public static void hommlet()
        {
            FadeAndTeleport(0, 0, 0, 5001, 619, 422);
            return;
        }
        public static void smith()
        {
            FadeAndTeleport(0, 0, 0, 5001, 577, 433);
            return;
        }
        public static void spugnoir()
        {
            FadeAndTeleport(0, 0, 0, 5007, 475, 482);
            return;
        }
        public static void courier()
        {
            FadeAndTeleport(0, 0, 0, 5009, 479, 483);
            return;
        }
        // the courier

        public static void templeagent()
        {
            // def templeagent(): #the courier
            FadeAndTeleport(0, 0, 0, 5009, 479, 483);
            return;
        }
        public static void traders()
        {
            FadeAndTeleport(0, 0, 0, 5010, 482, 480);
            return;
        }
        public static void rannos()
        {
            FadeAndTeleport(0, 0, 0, 5010, 482, 480);
            return;
        }
        public static void gremag()
        {
            FadeAndTeleport(0, 0, 0, 5010, 482, 480);
            return;
        }
        public static void rannosdavl()
        {
            FadeAndTeleport(0, 0, 0, 5010, 482, 480);
            return;
        }
        public static void terjon()
        {
            FadeAndTeleport(0, 0, 0, 5011, 486, 480);
            return;
        }
        public static void calmert()
        {
            FadeAndTeleport(0, 0, 0, 5012, 491, 484);
            return;
        }
        public static void burne()
        {
            FadeAndTeleport(0, 0, 0, 5016, 479, 482);
            return;
        }
        public static void rufus()
        {
            FadeAndTeleport(0, 0, 0, 5016, 479, 482);
            return;
        }
        public static void percy()
        {
            FadeAndTeleport(0, 0, 0, 5020, 478, 481);
            return;
        }
        public static void tarim()
        {
            FadeAndTeleport(0, 0, 0, 5022, 482, 481);
            return;
        }
        public static void mathilde()
        {
            FadeAndTeleport(0, 0, 0, 5023, 468, 484);
            return;
        }
        public static void meleny()
        {
            FadeAndTeleport(0, 0, 0, 5025, 485, 489);
            return;
        }
        public static void althea()
        {
            FadeAndTeleport(0, 0, 0, 5025, 485, 489);
            return;
        }
        public static void filliken()
        {
            FadeAndTeleport(0, 0, 0, 5025, 485, 489);
            return;
        }
        public static void bing()
        {
            FadeAndTeleport(0, 0, 0, 5026, 491, 487);
            return;
        }
        public static void tenants()
        {
            FadeAndTeleport(0, 0, 0, 5029, 485, 491);
            return;
        }
        public static void jinnerth()
        {
            FadeAndTeleport(0, 0, 0, 5030, 488, 484);
            return;
        }
        public static void jeweller()
        {
            FadeAndTeleport(0, 0, 0, 5033, 482, 488);
            return;
        }
        public static void jeweler()
        {
            FadeAndTeleport(0, 0, 0, 5033, 482, 488);
            return;
        }
        public static void nira()
        {
            FadeAndTeleport(0, 0, 0, 5033, 482, 488);
            return;
        }
        public static void niramelubb()
        {
            FadeAndTeleport(0, 0, 0, 5033, 482, 488);
            return;
        }
        public static void moneychanger()
        {
            FadeAndTeleport(0, 0, 0, 5033, 482, 488);
            return;
        }
        public static void brewhouse()
        {
            FadeAndTeleport(0, 0, 0, 5037, 475, 479);
            return;
        }
        public static void cavanaugh()
        {
            FadeAndTeleport(0, 0, 0, 5037, 475, 479);
            return;
        }
        public static void deklo()
        {
            FadeAndTeleport(0, 0, 0, 5069, 480, 480);
            return;
        }
        public static void inn()
        {
            FadeAndTeleport(0, 0, 0, 5007, 483, 480);
            return;
        }
        public static void welcomewench()
        {
            FadeAndTeleport(0, 0, 0, 5007, 483, 480);
            return;
        }
        public static void furnok()
        {
            FadeAndTeleport(0, 0, 0, 5007, 476, 478);
            return;
        }
        public static void ostler()
        {
            FadeAndTeleport(0, 0, 0, 5007, 482, 477);
            return;
        }
        public static void gundigoot()
        {
            FadeAndTeleport(0, 0, 0, 5007, 482, 477);
            return;
        }
        public static void gundi()
        {
            FadeAndTeleport(0, 0, 0, 5007, 482, 477);
            return;
        }
        public static void innkeeper()
        {
            FadeAndTeleport(0, 0, 0, 5007, 482, 477);
            return;
        }
        public static void castle()
        {
            FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            return;
        }
        public static void constructionsite()
        {
            FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            return;
        }
        public static void castleconstructionsite()
        {
            FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            return;
        }
        public static void jayfie()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            }
            else
            {
                jayfienight();
            }

            return;
        }
        public static void laborerspy()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            }
            else
            {
                jayfienight();
            }

            return;
        }
        public static void spy()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5001, 437, 698);
            }
            else
            {
                jayfienight();
            }

            return;
        }
        public static void jayfienight()
        {
            FadeAndTeleport(0, 0, 0, 5001, 383, 519);
            return;
        }
        public static void yvy()
        {
            FadeAndTeleport(0, 0, 0, 5001, 370, 533);
            return;
        }
        public static void tatooartist()
        {
            FadeAndTeleport(0, 0, 0, 5001, 370, 533);
            return;
        }
        public static void tatoo()
        {
            FadeAndTeleport(0, 0, 0, 5001, 370, 533);
            return;
        }
        public static void campmother()
        {
            FadeAndTeleport(0, 0, 0, 5001, 370, 533);
            return;
        }
        public static void laborercook()
        {
            FadeAndTeleport(0, 0, 0, 5001, 402, 533);
            return;
        }
        public static void dex()
        {
            FadeAndTeleport(0, 0, 0, 5001, 402, 533);
            return;
        }
        public static void elder()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void nevets()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void kenternevets()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void villageelder()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void townelder()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void kenter()
        {
            FadeAndTeleport(0, 0, 0, 5045, 482, 477);
            return;
        }
        public static void carpenter()
        {
            FadeAndTeleport(0, 0, 0, 5047, 476, 471);
            return;
        }
        public static void riklinkin()
        {
            FadeAndTeleport(0, 0, 0, 5047, 476, 471);
            return;
        }
        public static void marek()
        {
            FadeAndTeleport(0, 0, 0, 5047, 476, 471);
            return;
        }
        public static void barnmaker()
        {
            FadeAndTeleport(0, 0, 0, 5047, 476, 471);
            return;
        }
        public static void wainwright()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5044, 494, 487);
            }
            else
            {
                inn();
            }

            return;
        }
        public static void valden()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5044, 494, 487);
            }
            else
            {
                inn();
            }

            return;
        }
        public static void stonemason()
        {
            FadeAndTeleport(0, 0, 0, 5001, 455, 520);
            return;
        }
        public static void mason()
        {
            FadeAndTeleport(0, 0, 0, 5001, 455, 520);
            return;
        }
        public static void gister()
        {
            FadeAndTeleport(0, 0, 0, 5001, 455, 520);
            return;
        }
        public static void gisternoshim()
        {
            FadeAndTeleport(0, 0, 0, 5001, 455, 520);
            return;
        }
        public static void jay()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5001, 540, 249);
            }
            else
            {
                FadeAndTeleport(0, 0, 0, 5038, 487, 485);
            }

            return;
        }
        public static void blackjay()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5001, 540, 249);
            }
            else
            {
                FadeAndTeleport(0, 0, 0, 5038, 487, 485);
            }

            return;
        }
        public static void teamster()
        {
            FadeAndTeleport(0, 0, 0, 5032, 474, 482);
            return;
        }
        public static void sef()
        {
            FadeAndTeleport(0, 0, 0, 5032, 474, 482);
            return;
        }
        public static void sefflettner()
        {
            FadeAndTeleport(0, 0, 0, 5032, 474, 482);
            return;
        }
        public static void teamsterson()
        {
            if (!Utilities.is_daytime() && !GetGlobalFlag(4))
            {
                FadeAndTeleport(0, 0, 0, 5001, 561, 225);
            }
            else
            {
                FadeAndTeleport(0, 0, 0, 5032, 471, 485);
            }

            return;
        }
        public static void corl()
        {
            if (!Utilities.is_daytime() && !GetGlobalFlag(4))
            {
                FadeAndTeleport(0, 0, 0, 5001, 561, 225);
            }
            else
            {
                FadeAndTeleport(0, 0, 0, 5032, 471, 485);
            }

            return;
        }
        public static void jaroo()
        {
            FadeAndTeleport(0, 0, 0, 5042, 491, 474);
            return;
        }
        public static void druid()
        {
            FadeAndTeleport(0, 0, 0, 5042, 491, 474);
            return;
        }
        public static void renton()
        {
            FadeAndTeleport(0, 0, 0, 5063, 470, 477);
            return;
        }
        public static void hall()
        {
            FadeAndTeleport(0, 0, 0, 5001, 577, 410);
            return;
        }
        public static void townhall()
        {
            FadeAndTeleport(0, 0, 0, 5001, 577, 410);
            return;
        }
        public static void council()
        {
            FadeAndTeleport(0, 0, 0, 5001, 577, 410);
            return;
        }
        public static void prison()
        {
            FadeAndTeleport(0, 0, 0, 5014, 482, 478);
            return;
        }
        public static void chase()
        {
            FadeAndTeleport(0, 0, 0, 5001, 344, 498);
            return;
        }
        public static void trail()
        {
            FadeAndTeleport(0, 0, 0, 5001, 344, 498);
            return;
        }
        public static void emridygiant()
        {
            FadeAndTeleport(0, 0, 0, 5094, 611, 513);
            return;
        }
        public static void rainbowrock()
        {
            FadeAndTeleport(0, 0, 0, 5094, 487, 503);
            return;
        }
        public static void emridy()
        {
            FadeAndTeleport(0, 0, 0, 5094, 544, 411);
            return;
        }
        // MOATHOUSE			#
        // #

        public static void moathouse()
        {
            FadeAndTeleport(0, 0, 0, 5002, 480, 546);
            return;
        }
        public static void moatgate()
        {
            FadeAndTeleport(0, 0, 0, 5002, 487, 485);
            return;
        }
        public static void moathousegate()
        {
            FadeAndTeleport(0, 0, 0, 5002, 487, 485);
            return;
        }
        public static void moattower()
        {
            FadeAndTeleport(0, 0, 0, 5003, 471, 486);
            return;
        }
        public static void moathousetower()
        {
            FadeAndTeleport(0, 0, 0, 5003, 471, 486);
            return;
        }
        public static void lubash()
        {
            FadeAndTeleport(0, 0, 0, 5005, 425, 415);
            return;
        }
        public static void moatlevel1()
        {
            FadeAndTeleport(0, 0, 0, 5004, 476, 477);
            return;
        }
        public static void moathouseupperlevel()
        {
            moatlevel1();
            return;
        }
        public static void moatupper()
        {
            moatlevel1();
            return;
        }
        public static void moatinterior()
        {
            moatlevel1();
            return;
        }
        public static void moathouseinterior()
        {
            moatlevel1();
            return;
        }
        public static void moatstirges()
        {
            moatlevel1();
            return;
        }
        public static void raul()
        {
            FadeAndTeleport(0, 0, 0, 5004, 476, 477);
            return;
        }
        public static void raulthegrim()
        {
            FadeAndTeleport(0, 0, 0, 5004, 476, 477);
            return;
        }
        public static void moatzombies()
        {
            FadeAndTeleport(0, 0, 0, 5005, 420, 411);
            return;
        }
        public static void moatbugbears()
        {
            FadeAndTeleport(0, 0, 0, 5005, 444, 499);
            return;
        }
        public static void moatgnolls()
        {
            FadeAndTeleport(0, 0, 0, 5005, 509, 503);
            return;
        }
        public static void moathousegnolls()
        {
            FadeAndTeleport(0, 0, 0, 5005, 509, 503);
            return;
        }
        public static void moatsarge()
        {
            FadeAndTeleport(0, 0, 0, 5005, 536, 547);
            return;
        }
        public static void moathousesarge()
        {
            FadeAndTeleport(0, 0, 0, 5005, 536, 547);
            return;
        }
        public static void larethsarge()
        {
            FadeAndTeleport(0, 0, 0, 5005, 536, 547);
            return;
        }
        public static void larethsergeant()
        {
            FadeAndTeleport(0, 0, 0, 5005, 536, 547);
            return;
        }
        public static void lareth()
        {
            FadeAndTeleport(0, 0, 0, 5005, 475, 546);
            return;
        }
        // NULB				#

        public static void nulb()
        {
            FadeAndTeleport(0, 0, 0, 5051, 505, 367);
            return;
        }
        public static void imeryds()
        {
            FadeAndTeleport(0, 0, 0, 5068, 477, 482);
            return;
        }
        public static void otis()
        {
            FadeAndTeleport(0, 0, 0, 5051, 467, 527);
            return;
        }
        public static void mary()
        {
            FadeAndTeleport(0, 0, 0, 5058, 502, 479);
            return;
        }
        public static void riana()
        {
            FadeAndTeleport(0, 0, 0, 5058, 498, 489);
            return;
        }
        public static void ophelia()
        {
            FadeAndTeleport(0, 0, 0, 5057, 488, 489);
            return;
        }
        public static void madamophelia()
        {
            FadeAndTeleport(0, 0, 0, 5057, 488, 489);
            return;
        }
        public static void madam()
        {
            FadeAndTeleport(0, 0, 0, 5057, 488, 489);
            return;
        }
        public static void brothel()
        {
            FadeAndTeleport(0, 0, 0, 5057, 488, 489);
            return;
        }
        public static void snakepit()
        {
            FadeAndTeleport(0, 0, 0, 5057, 488, 489);
            return;
        }
        public static void serena()
        {
            FadeAndTeleport(0, 0, 0, 5051, 558, 529);
            return;
        }
        public static void sammy()
        {
            FadeAndTeleport(0, 0, 0, 5051, 495, 525);
            return;
        }
        public static void mona()
        {
            FadeAndTeleport(0, 0, 0, 5051, 558, 536);
            return;
        }
        public static void charlotte()
        {
            FadeAndTeleport(0, 0, 0, 5058, 494, 473);
            return;
        }
        public static void jenelda()
        {
            FadeAndTeleport(0, 0, 0, 5059, 483, 480);
            return;
        }
        public static void boatmanstavern()
        {
            FadeAndTeleport(0, 0, 0, 5051, 399, 522);
            return;
        }
        public static void tavern()
        {
            FadeAndTeleport(0, 0, 0, 5051, 399, 522);
            return;
        }
        public static void skole()
        {
            FadeAndTeleport(0, 0, 0, 5052, 477, 489);
            return;
        }
        public static void lodriss()
        {
            FadeAndTeleport(0, 0, 0, 5052, 472, 482);
            return;
        }
        public static void tolub()
        {
            FadeAndTeleport(0, 0, 0, 5052, 480, 479);
            return;
        }
        public static void grud()
        {
            FadeAndTeleport(0, 0, 0, 5051, 382, 483);
            return;
        }
        public static void grudsquinteye()
        {
            FadeAndTeleport(0, 0, 0, 5051, 382, 483);
            return;
        }
        public static void docks()
        {
            FadeAndTeleport(0, 0, 0, 5051, 382, 483);
            return;
        }
        public static void nulbdocks()
        {
            FadeAndTeleport(0, 0, 0, 5051, 382, 483);
            return;
        }
        public static void prestonwetz()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void residentialarea()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void nulbresidentialarea()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void nulbhouse()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void nulbhouses()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void preston()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void wetz()
        {
            FadeAndTeleport(0, 0, 0, 5051, 478, 470);
            return;
        }
        public static void hostel()
        {
            FadeAndTeleport(0, 0, 0, 5051, 556, 453);
            return;
        }
        public static void watersidehostel()
        {
            FadeAndTeleport(0, 0, 0, 5051, 556, 453);
            return;
        }
        public static void waterside()
        {
            FadeAndTeleport(0, 0, 0, 5051, 556, 453);
            return;
        }
        public static void alira()
        {
            FadeAndTeleport(0, 0, 0, 5060, 477, 503);
            return;
        }
        public static void pearl()
        {
            FadeAndTeleport(0, 0, 0, 5060, 479, 500);
            return;
        }
        public static void dala()
        {
            FadeAndTeleport(0, 0, 0, 5060, 477, 503);
            return;
        }
        public static void rentsch()
        {
            FadeAndTeleport(0, 0, 0, 5060, 478, 493);
            return;
        }
        public static void wat()
        {
            FadeAndTeleport(0, 0, 0, 5060, 481, 493);
            return;
        }
        public static void motherscreng()
        {
            FadeAndTeleport(0, 0, 0, 5053, 483, 484);
            return;
        }
        public static void screng()
        {
            FadeAndTeleport(0, 0, 0, 5053, 483, 484);
            return;
        }
        public static void ydey()
        {
            FadeAndTeleport(0, 0, 0, 5053, 483, 484);
            return;
        }
        public static void herbshop()
        {
            FadeAndTeleport(0, 0, 0, 5053, 483, 484);
            return;
        }
        public static void murfles()
        {
            FadeAndTeleport(0, 0, 0, 5054, 477, 487);
            return;
        }
        public static void hruda()
        {
            FadeAndTeleport(0, 0, 0, 5054, 477, 487);
            return;
        }
        public static void mickey()
        {
            FadeAndTeleport(0, 0, 0, 5051, 556, 472);
            return;
        }
        // TEMPLE			#

        public static void temple()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void templelevel0()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void templeoutside()
        {
            FadeAndTeleport(0, 0, 0, 5062, 516, 458);
            return;
        }
        public static void templeentrance()
        {
            FadeAndTeleport(0, 0, 0, 5062, 516, 458);
            return;
        }
        public static void bronzedoors()
        {
            FadeAndTeleport(0, 0, 0, 5062, 516, 458);
            return;
        }
        public static void stairs()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void templestaircase()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void secretstaircase()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void staircase()
        {
            FadeAndTeleport(0, 0, 0, 5064, 480, 484);
            return;
        }
        public static void templelevel1()
        {
            FadeAndTeleport(0, 0, 0, 5066, 524, 564);
            return;
        }
        public static void romag()
        {
            FadeAndTeleport(0, 0, 0, 5066, 460, 448);
            return;
        }
        public static void earthcommander()
        {
            FadeAndTeleport(0, 0, 0, 5066, 450, 462);
            return;
        }
        public static void earthcom()
        {
            FadeAndTeleport(0, 0, 0, 5066, 450, 462);
            return;
        }
        public static void earthtroopcommander()
        {
            FadeAndTeleport(0, 0, 0, 5066, 450, 462);
            return;
        }
        public static void wonnilonhideout()
        {
            FadeAndTeleport(0, 0, 0, 5066, 417, 381);
            return;
        }
        public static void hideout()
        {
            FadeAndTeleport(0, 0, 0, 5066, 417, 381);
            return;
        }
        public static void wonnilon()
        {
            FadeAndTeleport(0, 0, 0, 5066, 551, 423);
            return;
        }
        public static void wonni()
        {
            FadeAndTeleport(0, 0, 0, 5066, 551, 423);
            return;
        }
        public static void earthaltar()
        {
            FadeAndTeleport(0, 0, 0, 5066, 471, 387);
            return;
        }
        public static void turnkey()
        {
            FadeAndTeleport(0, 0, 0, 5066, 541, 453);
            return;
        }
        public static void harpies()
        {
            FadeAndTeleport(0, 0, 0, 5066, 424, 557);
            return;
        }
        public static void morgan()
        {
            FadeAndTeleport(0, 0, 0, 5066, 553, 567);
            return;
        }
        public static void templelevel2()
        {
            FadeAndTeleport(0, 0, 0, 5067, 568, 382);
            return;
        }
        public static void lvl2bugbears()
        {
            FadeAndTeleport(0, 0, 0, 5067, 423, 405);
            return;
        }
        public static void feldrin()
        {
            FadeAndTeleport(0, 0, 0, 5067, 524, 454);
            return;
        }
        public static void brunk()
        {
            FadeAndTeleport(0, 0, 0, 5067, 524, 454);
            return;
        }
        public static void oohlgrist()
        {
            FadeAndTeleport(0, 0, 0, 5067, 473, 612);
            return;
        }
        public static void aern()
        {
            FadeAndTeleport(0, 0, 0, 5067, 465, 562);
            return;
        }
        public static void bassanio()
        {
            FadeAndTeleport(0, 0, 0, 5067, 433, 540);
            return;
        }
        public static void tillahi()
        {
            FadeAndTeleport(0, 0, 0, 5067, 413, 439);
            return;
        }
        public static void countess()
        {
            FadeAndTeleport(0, 0, 0, 5067, 413, 439);
            return;
        }
        public static void alrrem()
        {
            // this is the proper spelling...
            FadeAndTeleport(0, 0, 0, 5067, 416, 499);
            return;
        }
        public static void allrem()
        {
            // ... but for the sake of convenience...
            FadeAndTeleport(0, 0, 0, 5067, 416, 499);
            return;
        }
        public static void cave()
        {
            FadeAndTeleport(0, 0, 0, 5113, 478, 517);
            return;
        }
        public static void ogrecave()
        {
            FadeAndTeleport(0, 0, 0, 5113, 478, 517);
            return;
        }
        public static void tubal()
        {
            FadeAndTeleport(0, 0, 0, 5067, 416, 499);
            return;
        }
        public static void antonio()
        {
            FadeAndTeleport(0, 0, 0, 5067, 416, 499);
            return;
        }
        public static void werewolves()
        {
            FadeAndTeleport(0, 0, 0, 5067, 464, 482);
            return;
        }
        public static void belsornig()
        {
            FadeAndTeleport(0, 0, 0, 5067, 534, 518);
            return;
        }
        public static void kelno()
        {
            FadeAndTeleport(0, 0, 0, 5067, 524, 484);
            return;
        }
        public static void minotaur()
        {
            FadeAndTeleport(0, 0, 0, 5067, 568, 382);
            return;
        }
        public static void littlesttroll()
        {
            FadeAndTeleport(0, 0, 0, 5067, 475, 397);
            return;
        }
        public static void thrommel()
        {
            FadeAndTeleport(0, 0, 0, 5105, 554, 449);
            return;
        }
        public static void leucrottas()
        {
            FadeAndTeleport(0, 0, 0, 5105, 423, 590);
            return;
        }
        public static void lamia()
        {
            FadeAndTeleport(0, 0, 0, 5105, 603, 608);
            return;
        }
        public static void whitman()
        {
            FadeAndTeleport(0, 0, 0, 5105, 420, 571);
            return;
        }
        public static void mandy()
        {
            FadeAndTeleport(0, 0, 0, 5105, 420, 571);
            return;
        }
        public static void smigmal()
        {
            FadeAndTeleport(0, 0, 0, 5105, 632, 470);
            return;
        }
        public static void falrinth()
        {
            FadeAndTeleport(0, 0, 0, 5105, 632, 470);
            return;
        }
        public static void scorpp()
        {
            FadeAndTeleport(0, 0, 0, 5105, 553, 490);
            return;
        }
        public static void templelevel4()
        {
            FadeAndTeleport(0, 0, 0, 5080, 481, 580);
            return;
        }
        public static void kella()
        {
            FadeAndTeleport(0, 0, 0, 5080, 538, 612);
            return;
        }
        public static void paida()
        {
            FadeAndTeleport(0, 0, 0, 5080, 594, 539);
            return;
        }
        public static void deggum()
        {
            FadeAndTeleport(0, 0, 0, 5080, 421, 535);
            return;
        }
        public static void senshock()
        {
            FadeAndTeleport(0, 0, 0, 5080, 376, 547);
            return;
        }
        public static void hedrack()
        {
            FadeAndTeleport(0, 0, 0, 5080, 479, 471);
            return;
        }
        public static void zuggtmoy()
        {
            FadeAndTeleport(0, 0, 0, 5079, 541, 503);
            return;
        }
        public static void gemthrone()
        {
            FadeAndTeleport(0, 0, 0, 5079, 574, 480);
            return;
        }
        public static void templetower()
        {
            FadeAndTeleport(0, 0, 0, 5111, 485, 507);
            return;
        }
        public static void earthtemple()
        {
            FadeAndTeleport(0, 0, 0, 5066, 524, 564);
            return;
        }
        public static void ogrechief()
        {
            FadeAndTeleport(0, 0, 0, 5066, 484, 535);
            return;
        }
        public static void earthogrechief()
        {
            FadeAndTeleport(0, 0, 0, 5066, 484, 535);
            return;
        }
        public static void gnollleader()
        {
            FadeAndTeleport(0, 0, 0, 5066, 484, 535);
            return;
        }
        public static void airaltar()
        {
            FadeAndTeleport(0, 0, 0, 5067, 495, 499);
            return;
        }
        // NODES				#

        public static void airnode()
        {
            FadeAndTeleport(0, 0, 0, 5081, 480, 480);
            return;
        }
        public static void ashrem()
        {
            FadeAndTeleport(0, 0, 0, 5081, 538, 396);
            return;
        }
        public static void taki()
        {
            FadeAndTeleport(0, 0, 0, 5081, 417, 553);
            return;
        }
        public static void vrock()
        {
            FadeAndTeleport(0, 0, 0, 5081, 411, 398);
            return;
        }
        public static void firenode()
        {
            FadeAndTeleport(0, 0, 0, 5083, 503, 496);
            return;
        }
        public static void darley()
        {
            FadeAndTeleport(0, 0, 0, 5083, 569, 529);
            return;
        }
        public static void balor()
        {
            FadeAndTeleport(0, 0, 0, 5083, 455, 387);
            return;
        }
        public static void waternode()
        {
            FadeAndTeleport(0, 0, 0, 5084, 523, 474);
            return;
        }
        public static void hezrou()
        {
            FadeAndTeleport(0, 0, 0, 5084, 419, 447);
            return;
        }
        public static void grank()
        {
            FadeAndTeleport(0, 0, 0, 5084, 520, 484);
            return;
        }
        public static void earthnode()
        {
            FadeAndTeleport(0, 0, 0, 5082, 483, 472);
            return;
        }
        public static void jaer()
        {
            FadeAndTeleport(0, 0, 0, 5082, 438, 549);
            return;
        }
        public static void sargen()
        {
            FadeAndTeleport(0, 0, 0, 5082, 415, 481);
            return;
        }
        // VERBOBONC			#

        public static void verbobonc()
        {
            FadeAndTeleport(0, 0, 0, 5121, 228, 507);
            return;
        }
        public static void verbo()
        {
            FadeAndTeleport(0, 0, 0, 5121, 228, 507);
            return;
        }
        public static void vbbc()
        {
            FadeAndTeleport(0, 0, 0, 5121, 228, 507);
            return;
        }
        public static void verbocono()
        {
            FadeAndTeleport(0, 0, 0, 5121, 228, 507);
            return;
        }
        public static void darlia()
        {
            FadeAndTeleport(0, 0, 0, 5156, 472, 476);
            return;
        }
        public static void viscount()
        {
            if (Utilities.is_daytime())
            {
                FadeAndTeleport(0, 0, 0, 5170, 497, 484);
            }
            else
            {
                FadeAndTeleport(0, 0, 0, 5122, 478, 481);
            }

            return;
        }
        public static void wilfrick()
        {
            viscount();
        }
        public static void welkwood()
        {
            FadeAndTeleport(0, 0, 0, 5093, 516, 323);
            return;
        }
        public static void welkwoodexterior()
        {
            welkwood();
            return;
        }
        public static void welkwoodbog()
        {
            welkwood();
            return;
        }
        public static void welkwood_bog()
        {
            welkwood();
            return;
        }
        public static void bog()
        {
            welkwood();
            return;
        }

    }
}
