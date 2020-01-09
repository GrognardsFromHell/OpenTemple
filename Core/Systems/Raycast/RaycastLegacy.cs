using System;
using System.Net.WebSockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;

namespace OpenTemple.Core.Systems.Raycast
{
    public static class RaycastLegacy
    {

        [TempleDllLocation(0x100bace0)]
public static unsafe int Raycast(RaycastPacket objIt)
{
  LocAndOffsets tgtLoc;
  float v15;
  float v16;
  float v17;
  ulong v30;
  int v37;
  int v38;
  int v39;
  int v48;
  LocAndOffsets v51;
  RaycastFlag v60;
  int result;
  LocAndOffsets v67;
  RaycastFlag v78;
  LocAndOffsets v79;
  RaycastFlag v89;
  LocAndOffsets v90;
  RaycastFlag v101;
  RaycastFlag v112;
  LocAndOffsets v113;
  RaycastFlag v124;
  LocAndOffsets v125;
  RaycastFlag v136;
  LocAndOffsets v137;
  LocAndOffsets v149;
  RaycastFlag v160;
  int v162;
  byte v163;
  int v171;
  GameObjectBody v172;
  int v173;
  LocAndOffsets objloc;
  float v178;
  int v179;
  GameObjectBody v180;
  float v181;
  float v182;
  int v188;
  float absY;
  float absX;
  PointAlongSegment v239;
  float v240;
  int v243;
  int v245;
  int v246;
  int v248;
  int v249;

  bool[] v257 = new bool[4];
  LocAndOffsets a2;
  LockedMapSector[] sectorOut = new LockedMapSector[4];
  int[] v281 = new int[4];
  TileRect xyRect;
  int[] v283 = new int[4];

  GameObjectBody ray_src_handle = null;
  var v259 = 0;
  GameObjectBody ray_target_handle = null;
  var v263 = 0;
  if ( (objIt.flags & RaycastFlag.HasSourceObj) != 0 )
  {
    ray_src_handle = objIt.sourceObj;
  }
  if ( (objIt.flags & RaycastFlag.HasTargetObj) != 0 )
  {
    ray_target_handle = objIt.target;
  }
  if ( (objIt.flags & RaycastFlag.HasRadius) == 0 )
  {
    objIt.radius = 0.1f;
  }
  var srchPkt = new RaycastPointSearchPacket();
  srchPkt.origin = objIt.origin.ToInches2D();

  // var v6 = &objIt.targetLoc.location.locx;
  srchPkt.target = objIt.targetLoc.ToInches2D();

  float deltaX = srchPkt.target.X - srchPkt.origin.X;
  srchPkt.radius = objIt.radius;
  float deltaY = srchPkt.target.Y - srchPkt.origin.Y;
  srchPkt.rangeInch = MathF.Sqrt(deltaY * deltaY + deltaX * deltaX);
  var rangeInverse = 1.0f / srchPkt.rangeInch;
  srchPkt.direction.X = rangeInverse * deltaX;
  srchPkt.direction.Y = rangeInverse * deltaY;
  srchPkt.absOdotU = srchPkt.direction.Y * srchPkt.origin.Y + srchPkt.direction.X * srchPkt.origin.X;
  if ((objIt.flags & RaycastFlag.HasRangeLimit) != 0 )
  {
    float v11 = srchPkt.direction.X * objIt.rayRangeInches;
    srchPkt.rangeInch = objIt.rayRangeInches;
    srchPkt.target.X = v11 + srchPkt.origin.X;
    srchPkt.target.Y = srchPkt.direction.Y * objIt.rayRangeInches + srchPkt.origin.Y;
    objIt.targetLoc = LocAndOffsets.FromInches(srchPkt.target);
  }
  var v12 = objIt.targetLoc.location.locy;
  a2.location.locx = objIt.targetLoc.location.locx;
  var v13 = objIt.targetLoc.off_x;
  a2.location.locy = v12;
  a2.off_y = objIt.targetLoc.off_y;
  var flags = objIt.flags;
  a2.off_x = v13;
  SearchPointAlongRay v242 = RaycastPacket.GetPointAlongSegmentNearestToOriginDistanceRfromV;
  if ( (flags & RaycastFlag.GetObjIntersection) == 0 )
  {
    v242 = RaycastPacket.IsPointCloseToSegment;
  }
  if ( srchPkt.origin.X <= (float)srchPkt.target.X )
  {
    v15 = srchPkt.origin.X;
  }
  else
  {
    v15 = srchPkt.target.X;
  }

  if ( srchPkt.origin.Y <= srchPkt.target.Y )
  {
    v16 = srchPkt.origin.Y;
  }
  else
  {
    v16 = srchPkt.target.Y;
  }
  if ( srchPkt.origin.X >= srchPkt.target.X )
  {
    v17 = srchPkt.origin.X;
  }
  else
  {
    v17 = srchPkt.target.X;
  }

  float rectY;
  if ( srchPkt.origin.Y >= srchPkt.target.Y )
  {
    rectY = srchPkt.origin.Y;
  }
  else
  {
    rectY = srchPkt.target.Y;
  }
  var locXy = locXY.FromInches(new Vector2(v15 - 150.0f, v16 - 150.0f));
  var v278 = locXY.FromInches(new Vector2(v17 + 150.0f, rectY + 150.0f));
  xyRect.x1 = locXy.locx;
  xyRect.y1 = locXy.locy;
  xyRect.x2 = v278.locx;
  xyRect.y2 = v278.locy;
  TileFlags block_x0y0 = TileFlags.BlockX0Y0;
  TileFlags block_x1y0 = TileFlags.BlockX1Y0;
  TileFlags block_x2y0 = TileFlags.BlockX2Y0;
  TileFlags block_x0y1 = TileFlags.BlockX0Y1;
  TileFlags block_x1y1 = TileFlags.BlockX1Y1;
  TileFlags block_x2y1 = TileFlags.BlockX2Y1;
  TileFlags block_x0y2 = TileFlags.BlockX0Y2;
  TileFlags block_x1y2 = TileFlags.BlockX1Y2;
  TileFlags block_x2y2 = TileFlags.BlockX2Y2;
  if ( ((objIt.flags & RaycastFlag.IgnoreFlyover)) == 0 )
  {
    block_x0y0 |= TileFlags.FlyOverX0Y0;
    block_x1y0 |= TileFlags.FlyOverX1Y0;
    block_x2y0 |= TileFlags.FlyOverX2Y0;
    block_x0y1 |= TileFlags.FlyOverX0Y1;
    block_x1y1 |= TileFlags.FlyOverX1Y1;
    block_x2y1 |= TileFlags.FlyOverX2Y1;
    block_x0y2 |= TileFlags.FlyOverX0Y2;
    block_x1y2 |= TileFlags.FlyOverX1Y2;
    block_x2y2 |= TileFlags.FlyOverX2Y2;
  }

  if ( !BuildTileListFromRect(xyRect, out var tle) )
  {
    return 0;
  }
  var v20 = objIt.targetLoc.location.locx;
  var v21 = objIt.origin.location.locx;
  var v256 = objIt.origin.location.locy;
  var v252 = objIt.targetLoc.location.locy;
  var v271 = v21;
  var v270 = v20;
  var v253 = 0;
  if ( v20 < v21 )
  {
    var v277 = 0;
    v270 = v21;
    v271 = v20;
  }
  if ( v252 < v256 )
  {
    var v22 = v252;
    v253 = 0;
    v252 = v256;
    v256 = v22;
  }
  int v23 = 1 - (int)(objIt.radius * -0.035355341f);
  v271 -= v23;
  v256 -= v23;
  v270 += v23;
  v252 += v23;
  var v250 = 0;
  if (tle.Ny <= 0)
  {
    return objIt.Count;
  }

  TLESub v236;
  LOOP1:
    var v25 = 0;
    v236 = tle.tlesub[v250];
    if (tle.tlesub[v250].Nx > 0)
    {
      do
      {
        v281[v25] = tle.tlesub[v250].sectorTileCoords[v25];
        v283[v25] = 64 - tle.tlesub[v250].deltas[v25];
        var sectorLoc = tle.tlesub[v250].sectorLocs[v25];
        sectorOut[v25] = new LockedMapSector(sectorLoc);
        v257[v25] = sectorOut[v25].IsValid;
        ++v25;
      } while (v25 < tle.tlesub[v250].Nx);
    }

    var v31 = tle.tlesub[v250].Nx == 0;
    var v9 = tle.tlesub[v250].Nx < 0;
    v245 = 0;
    if (!v9 && !v31)
    {
      goto LOOP1_END;
    }

    LABEL_195:
    var v164 = v236;
    var v165 = v236.field_68;
    v246 = 0;
    if (v165 > 0)
    {
      do
      {
        var v166 = 0;
        v31 = v164.Nx == 0;
        v9 = v164.Nx < 0;
        v245 = 0;
        if (!v9 && !v31)
        {
          int[] v167 = v164.deltas;
          do
          {
            if (v257[v166])
            {
              v31 = v167[v166] == 0;
              v9 = v167[v166] < 0;
              v249 = 0;
              if (!v9 && !v31)
              {
                v248 = v281[v166];
                do
                {
                  Sector.GetSectorTileFromIndex(v248, out var secTileX, out var secTileY);
                  var objects = sectorOut[v166].Sector.objects.tiles[secTileX, secTileY];
                  if (objects != null)
                  {
                    foreach (var v169 in objects)
                    {
                      if ((v169.GetFlags() &
                           (ObjectFlag.NO_BLOCK | ObjectFlag.OFF | ObjectFlag.DESTROYED)) == 0)
                      {
                        if ((v169 != ray_src_handle) && (v169 != ray_target_handle))
                        {
                          if (v169.IsSecretDoor() || !GameSystems.MapObject.IsUntargetable(v169))
                          {
                            var v174 = v169.type;
                            if ((objIt.flags & RaycastFlag.ExcludeItemObjects) == 0 || !v174.IsEquipment())
                            {
                              if ((objIt.flags & RaycastFlag.ExcludePortals) == 0 || v174 != ObjectType.portal)
                              {
                                objloc = v169.GetLocationFull();
                                float radois = v169.GetRadius();
                                if (v242(
                                  objloc.ToInches2D(),
                                  radois,
                                  in srchPkt,
                                  out v239))
                                {
                                  if ((objIt.flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) == 0
                                      || IsStuff(v169, objIt, ref a2))
                                  {
                                    var resultItem = new RaycastResultItem
                                    {
                                      obj = v169,
                                      loc = v169.GetLocationFull()
                                    };

                                    if ((objIt.flags & RaycastFlag.GetObjIntersection) != 0)
                                    {
                                      resultItem.intersectionPoint = LocAndOffsets.FromInches(v239.absX, v239.absY);
                                      resultItem.intersectionDistance = v239.distFromOrigin;
                                    }

                                    objIt.results.Add(resultItem);
                                  }
                                }
                              }
                            }
                          }
                        }
                      }

                    }

                    v166 = v245;
                    v164 = v236;
                  }

                  ++v248;
                } while (++v249 - v167[v166] < 0);

                v281[v166] = v248;
              }

              v281[v166] += v283[v166];
            }

            ++v166;
            v9 = v166 - v164.Nx < 0;
            v245 = v166;
          } while (v9);
        }

        v188 = v164.field_68;
        ++v246;
      } while (v246 < v188);
    }

    foreach (var sector in sectorOut)
    {
      sector?.Dispose();
    }

    if (++v250 >= tle.Ny)
    {
      return objIt.Count;
    }

    goto LOOP1;
  LOOP1_END:

  LOOP2:
    if (v257[v245])
    {
      var v32 = sectorOut[v245];
      var v35 = v32.Loc.GetBaseTile();
      var v36 = v35;
      v37 = v271 - v35.locx;
      v38 = v270 - v36.locx;
      v39 = v252 - v35.locy;
      var v40 = v256 - v36.locy;
      if (v37 < 0)
      {
        v37 = 0;
      }

      if (v40 < 0)
      {
        v40 = 0;
      }

      if (v38 > 63)
      {
        v38 = 63;
      }

      if (v39 > 63)
      {
        v39 = 63;
      }

      v246 = v40;
      if (v40 <= v39)
      {
        goto LOOP2_END;
      }
    }

    LABEL_194:
    if (!(++v245 - v236.Nx < 0))
    {
      goto LABEL_195;
    }

    goto LOOP2;
  LOOP2_END:

  LOOP3:
  {
    v249 = v37;
    if (v37 <= v38)
    {
      goto LOOP3_END;
    }

    LABEL_193:
    if (++v246 > v39)
    {
      goto LABEL_194;
    }
  }
  goto LOOP3;
  LOOP3_END:

  foreach (var sector in sectorOut)
  {
    sector?.Dispose();
  }

  return 0;
//
//  while (1)
//  {
//    var v41 = (int) *(&sectorOut + v245);
//    var v42 = *(int*) (v41 + 12);
//    long v43 = *(_QWORD*) (v41 + 8);
//    ulong v44 = GetSectorLocBase /*0x10081a00*/(*(_QWORD*) (v41 + 8));
//    int v45 = v44;
//    int v46 = _longint_mul(v246 + (ulong) HIDWORD(v44), 0x100000000);
//    ulong v47 = v249 + (ulong) v45;
//    var v49 = HIDWORD(v47) | v48;
//    int v50 = v47 | v46;
//    TileFlags v233 = (*(&sectorOut + v245)).tilePkt.tiles[v249 + (v246 << 6)].flags;
//    if (v233 & block_x0y0)
//    {
//      *(_QWORD*) &v51.off_x = -4533196875127924363;
//      v51.location = (locXY) __PAIR__(v49, v50);
//      v51.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, IteratorSearchPacket *, float *))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x200 || (v233 & 0x8040000) == 134479872)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        var v52 = objIt.resultCount + 1;
//        objIt.resultCount = v52;
//        var v53 = (RaycastResultItem*) realloc(objIt.results, 56 * v52);
//        int v54 = objIt.resultCount;
//        objIt.results = v53;
//        *((int*) &v53[v54] - 8) = 0;
//        *((int*) &v53[v54] - 7) = 0;
//        RaycastResultItem* v55 = objIt.results;
//        int v56 = objIt.resultCount;
//        *((int*) &v55[v56] - 12) = v50;
//        *((int*) &v55[v56] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = -1055467147;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = -1055467147;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x200)
//        {
//          objIt.results[objIt.resultCount - 1].flags |= 2;
//        }
//
//        v243 = v233 & 0x40000;
//        if (v233 & 0x40000)
//        {
//          var v57 = &objIt.results[objIt.resultCount - 1];
//          v57.flags |= 4;
//        }
//
//        var v58 = objIt.flags;
//        if ((v58 & 0x200))
//        {
//          var v59 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v59[-1].intersectionPoint,
//            &v59[-1].intersectionPoint.off_x,
//            &v59[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v60 = objIt.flags, objIt.flags & 0x10) && v233 & 0x200 || v60 & 0x20 && *(float*) &v243 != 0.0)
//        {
//          var v61 = v236;
//          var v62 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v63 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v62])
//            {
//              var v64 = *(int*) (v63 + 4);
//              ulong v65 = *(_QWORD*) v63;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v63);
//              v61 = v236;
//            }
//
//            ++v62;
//            v63 += 8;
//          } while (v62 < *(int*) v61);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x1y0 & v233)
//    {
//      *(_QWORD*) &v67.off_x = -4533196878367424512;
//      v67.location = (locXY) __PAIR__(v49, v50);
//      v67.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, IteratorSearchPacket *, float *))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x400 || (v233 & 0x8080000) == 134742016)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v68 = 56 * (objIt.resultCount + 1);
//        RaycastResultItem* v69 = objIt.results;
//        ++objIt.resultCount;
//        var v70 = (RaycastResultItem*) realloc(v69, v68);
//        int v71 = objIt.resultCount;
//        objIt.results = v70;
//        v71 *= 56;
//        *(int*) ((string) v70 + v71 - 32) = 0;
//        *(int*) ((string) v70 + v71 - 28) = 0;
//        RaycastResultItem* v72 = objIt.results;
//        int v73 = objIt.resultCount;
//        *((int*) &v72[v73] - 12) = v50;
//        *((int*) &v72[v73] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 0;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = -1055467147;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x400)
//        {
//          objIt.results[objIt.resultCount - 1].flags |= 2;
//        }
//
//        int v74 = v233 & 0x80000;
//        v243 = v233 & 0x80000;
//        if (v233 & 0x80000)
//        {
//          var v75 = &objIt.results[objIt.resultCount - 1];
//          v75.flags |= 4;
//        }
//
//        var v76 = objIt.flags;
//        if ((v76 & 0x200))
//        {
//          var v77 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v77[-1].intersectionPoint,
//            &v77[-1].intersectionPoint.off_x,
//            &v77[-1].intersectionPoint.off_y);
//          v74 = v243;
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v78 = objIt.flags, objIt.flags & 0x10) && v233 & 0x400 || v78 & 0x20 && v74)
//        {
//          var v193 = v236;
//          var v194 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v195 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v194])
//            {
//              var v196 = *(int*) (v195 + 4);
//              ulong v197 = *(_QWORD*) v195;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v195);
//              v193 = v236;
//            }
//
//            ++v194;
//            v195 += 8;
//          } while (v194 < *(int*) v193);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x2y0 & v233)
//    {
//      *(_QWORD*) &v79.off_x = -4533196877275408011;
//      v79.location = (locXY) __PAIR__(v49, v50);
//      v79.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x800 || (v233 & 0x8100000) == 135266304)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v80 = 56 * (objIt.resultCount++ + 1);
//        var v81 = (RaycastResultItem*) realloc(objIt.results, v80);
//        int v82 = objIt.resultCount;
//        objIt.results = v81;
//        *((int*) &v81[v82] - 8) = 0;
//        *((int*) &v81[v82] - 7) = 0;
//        RaycastResultItem* v83 = objIt.results;
//        int v84 = objIt.resultCount;
//        *((int*) &v83[v84] - 12) = v50;
//        *((int*) &v83[v84] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 1092016501;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = -1055467147;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x800)
//        {
//          var v85 = &objIt.results[objIt.resultCount - 1];
//          v85.flags |= 2;
//        }
//
//        v243 = v233 & 0x100000;
//        if (v233 & 0x100000)
//        {
//          var v86 = &objIt.results[objIt.resultCount - 1];
//          v86.flags |= 4;
//        }
//
//        var v87 = objIt.flags;
//        if ((v87 & 0x200))
//        {
//          var v88 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v88[-1].intersectionPoint,
//            &v88[-1].intersectionPoint.off_x,
//            &v88[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v89 = objIt.flags, objIt.flags & 0x10) && v233 & 0x800 || v89 & 0x20 && *(float*) &v243 != 0.0)
//        {
//          var v198 = v236;
//          var v199 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v200 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v199])
//            {
//              var v201 = *(int*) (v200 + 4);
//              ulong v202 = *(_QWORD*) v200;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v200);
//              v198 = v236;
//            }
//
//            ++v199;
//            v200 += 8;
//          } while (v199 < *(int*) v198);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x0y1 & v233)
//    {
//      *(_QWORD*) &v90.off_x = 3239500149;
//      v90.location = (locXY) __PAIR__(v49, v50);
//      v90.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x1000 || (v233 & 0x8200000) == 136314880)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v91 = 56 * (objIt.resultCount++ + 1);
//        var v92 = (RaycastResultItem*) realloc(objIt.results, v91);
//        int v93 = objIt.resultCount;
//        objIt.results = v92;
//        *((int*) &v92[v93] - 8) = 0;
//        *((int*) &v92[v93] - 7) = 0;
//        RaycastResultItem* v94 = objIt.results;
//        int v95 = objIt.resultCount;
//        *((int*) &v94[v95] - 12) = v50;
//        *((int*) &v94[v95] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = -1055467147;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 0;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x1000)
//        {
//          var v96 = &objIt.results[objIt.resultCount - 1];
//          v96.flags |= 2;
//        }
//
//        int v97 = v233 & 0x200000;
//        v243 = v233 & 0x200000;
//        if (v233 & 0x200000)
//        {
//          var v98 = &objIt.results[objIt.resultCount - 1];
//          v98.flags |= 4;
//        }
//
//        var v99 = objIt.flags;
//        if ((v99 & 0x200))
//        {
//          var v100 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v100[-1].intersectionPoint,
//            &v100[-1].intersectionPoint.off_x,
//            &v100[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//          v97 = v243;
//        }
//
//        if ((v101 = objIt.flags, objIt.flags & 0x10) && v233 & 0x1000 || v101 & 0x20 && v97)
//        {
//          var v203 = v236;
//          var v204 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v205 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v204])
//            {
//              var v206 = *(int*) (v205 + 4);
//              ulong v207 = *(_QWORD*) v205;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v205);
//              v203 = v236;
//            }
//
//            ++v204;
//            v205 += 8;
//          } while (v204 < *(int*) v203);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x1y1 & v233)
//    {
//      (LocAndOffsets) __PAIR__((int) v49, v50).ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x2000 || (v233 & 0x8400000) == 138412032)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v102 = 56 * (objIt.resultCount + 1);
//        RaycastResultItem* v103 = objIt.results;
//        ++objIt.resultCount;
//        var v104 = (RaycastResultItem*) realloc(v103, v102);
//        int v105 = objIt.resultCount;
//        objIt.results = v104;
//        v105 *= 56;
//        *(int*) ((string) v104 + v105 - 32) = 0;
//        *(int*) ((string) v104 + v105 - 28) = 0;
//        RaycastResultItem* v106 = objIt.results;
//        int v107 = objIt.resultCount;
//        *((int*) &v106[v107] - 12) = v50;
//        *((int*) &v106[v107] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 0;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 0;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x2000)
//        {
//          objIt.results[objIt.resultCount - 1].flags |= 2;
//        }
//
//        int v108 = v233 & 0x400000;
//        v243 = v233 & 0x400000;
//        if (v233 & 0x400000)
//        {
//          var v109 = &objIt.results[objIt.resultCount - 1];
//          v109.flags |= 4;
//        }
//
//        var v110 = objIt.flags;
//        if ((v110 & 0x200))
//        {
//          var v111 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v111[-1].intersectionPoint,
//            &v111[-1].intersectionPoint.off_x,
//            &v111[-1].intersectionPoint.off_y);
//          v108 = v243;
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v112 = objIt.flags, objIt.flags & 0x10) && v233 & 0x2000 || v112 & 0x20 && v108)
//        {
//          var v208 = v236;
//          var v209 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v210 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v209])
//            {
//              var v211 = *(int*) (v210 + 4);
//              ulong v212 = *(_QWORD*) v210;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v210);
//              v208 = v236;
//            }
//
//            ++v209;
//            v210 += 8;
//          } while (v209 < *(int*) v208);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x2y1 & v233)
//    {
//      *(_QWORD*) &v113.off_x = 1092016501;
//      v113.location = (locXY) __PAIR__(v49, v50);
//      v113.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x4000 || (v233 & 0x8800000) == 142606336)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v114 = 56 * (objIt.resultCount++ + 1);
//        var v115 = (RaycastResultItem*) realloc(objIt.results, v114);
//        int v116 = objIt.resultCount;
//        objIt.results = v115;
//        *((int*) &v115[v116] - 8) = 0;
//        *((int*) &v115[v116] - 7) = 0;
//        RaycastResultItem* v117 = objIt.results;
//        int v118 = objIt.resultCount;
//        *((int*) &v117[v118] - 12) = v50;
//        *((int*) &v117[v118] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 1092016501;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 0;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x4000)
//        {
//          var v119 = &objIt.results[objIt.resultCount - 1];
//          v119.flags |= 2;
//        }
//
//        int v120 = v233 & 0x800000;
//        v243 = v233 & 0x800000;
//        if (v233 & 0x800000)
//        {
//          var v121 = &objIt.results[objIt.resultCount - 1];
//          v121.flags |= 4;
//        }
//
//        var v122 = objIt.flags;
//        if ((v122 & 0x200))
//        {
//          var v123 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v123[-1].intersectionPoint,
//            &v123[-1].intersectionPoint.off_x,
//            &v123[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//          v120 = v243;
//        }
//
//        if ((v124 = objIt.flags, objIt.flags & 0x10) && v233 & 0x4000 || v124 & 0x20 && v120)
//        {
//          var v213 = v236;
//          var v214 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v215 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v214])
//            {
//              var v216 = *(int*) (v215 + 4);
//              ulong v217 = *(_QWORD*) v215;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v215);
//              v213 = v236;
//            }
//
//            ++v214;
//            v215 += 8;
//          } while (v214 < *(int*) v213);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x0y2 & v233)
//    {
//      *(_QWORD*) &v125.off_x = 4690175161726851445;
//      v125.location = (locXY) __PAIR__(v49, v50);
//      v125.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        if (v233 & 0x8000 || (v233 & 0x9000000) == 150994944)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v126 = 56 * (objIt.resultCount + 1);
//        RaycastResultItem* v127 = objIt.results;
//        ++objIt.resultCount;
//        var v128 = (RaycastResultItem*) realloc(v127, v126);
//        int v129 = objIt.resultCount;
//        objIt.results = v128;
//        v129 *= 56;
//        *(int*) ((string) v128 + v129 - 32) = 0;
//        *(int*) ((string) v128 + v129 - 28) = 0;
//        RaycastResultItem* v130 = objIt.results;
//        int v131 = objIt.resultCount;
//        *((int*) &v130[v131] - 12) = v50;
//        *((int*) &v130[v131] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = -1055467147;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 1092016501;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v233 & 0x8000)
//        {
//          objIt.results[objIt.resultCount - 1].flags |= 2;
//        }
//
//        int v132 = v233 & 0x1000000;
//        v243 = v233 & 0x1000000;
//        if (v233 & 0x1000000)
//        {
//          var v133 = &objIt.results[objIt.resultCount - 1];
//          v133.flags |= 4;
//        }
//
//        var v134 = objIt.flags;
//        if ((v134 & 0x200))
//        {
//          var v135 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v135[-1].intersectionPoint,
//            &v135[-1].intersectionPoint.off_x,
//            &v135[-1].intersectionPoint.off_y);
//          v132 = v243;
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v136 = objIt.flags, objIt.flags & 0x10) && v233 & 0x8000 || v136 & 0x20 && v132)
//        {
//          var v218 = v236;
//          var v219 = 0;
//          if (*(int*) v236 <= 0)
//          {
//            return objIt.Count;
//          }
//
//          var v220 = (int) (v236 + 40);
//          do
//          {
//            if (v257[v219])
//            {
//              var v221 = *(int*) (v220 + 4);
//              ulong v222 = *(_QWORD*) v220;
//              GameSystems.MapSector.UnlockSector(*(_QWORD*) v220);
//              v218 = v236;
//            }
//
//            ++v219;
//            v220 += 8;
//          } while (v219 < *(int*) v218);
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (block_x1y2 & v233)
//    {
//      *(_QWORD*) &v137.off_x = 4690175158487351296;
//      v137.location = (locXY) __PAIR__(v49, v50);
//      v137.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        int v234 = v233 & 0x10000;
//        if (v233 & 0x10000 || (v233 & 0xA000000) == 167772160)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v138 = 56 * (objIt.resultCount++ + 1);
//        var v139 = (RaycastResultItem*) realloc(objIt.results, v138);
//        int v140 = objIt.resultCount;
//        objIt.results = v139;
//        *((int*) &v139[v140] - 8) = 0;
//        *((int*) &v139[v140] - 7) = 0;
//        RaycastResultItem* v141 = objIt.results;
//        int v142 = objIt.resultCount;
//        *((int*) &v141[v142] - 12) = v50;
//        *((int*) &v141[v142] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 0;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 1092016501;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v234)
//        {
//          var v143 = &objIt.results[objIt.resultCount - 1];
//          v143.flags |= 2;
//        }
//
//        int v144 = v233 & 0x2000000;
//        v243 = v233 & 0x2000000;
//        if (v233 & 0x2000000)
//        {
//          var v145 = &objIt.results[objIt.resultCount - 1];
//          v145.flags |= 4;
//        }
//
//        var v146 = objIt.flags;
//        if ((v146 & 0x200))
//        {
//          var v147 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v147[-1].intersectionPoint,
//            &v147[-1].intersectionPoint.off_x,
//            &v147[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//          v144 = v243;
//        }
//
//        var v148 = objIt.flags;
//        if (objIt.flags & 0x10)
//        {
//          if (v234)
//          {
//            break;
//          }
//        }
//
//        if (v148 & 0x20 && v144)
//        {
//          break;
//        }
//      }
//    }
//
//    if (block_x2y2 & v233)
//    {
//      *(_QWORD*) &v149.off_x = 4690175159579367797;
//      v149.location = (locXY) __PAIR__(v49, v50);
//      v149.ToInches2D(&absX, &absY);
//      if (((int(*)(int, int, int, int, int))v242)(
//        LODWORD(absX),
//        LODWORD(absY),
//        1083627893,
//        &srchPkt,
//        &v239) )
//      {
//        int v235 = v233 & 0x20000;
//        if (v233 & 0x20000 || (v233 & 0xC000000) == 201326592)
//        {
//          objIt.flags |= 0x80000000;
//        }
//
//        size_t v150 = 56 * (objIt.resultCount + 1);
//        RaycastResultItem* v151 = objIt.results;
//        ++objIt.resultCount;
//        var v152 = (RaycastResultItem*) realloc(v151, v150);
//        int v153 = objIt.resultCount;
//        objIt.results = v152;
//        *((int*) &v152[v153] - 8) = 0;
//        *((int*) &v152[v153] - 7) = 0;
//        RaycastResultItem* v154 = objIt.results;
//        int v155 = objIt.resultCount;
//        *((int*) &v154[v155] - 12) = v50;
//        *((int*) &v154[v155] - 11) = v49;
//        *((int*) &objIt.results[objIt.resultCount] - 10) = 1092016501;
//        *((int*) &objIt.results[objIt.resultCount] - 9) = 1092016501;
//        objIt.results[objIt.resultCount - 1].flags = 0;
//        if (v235)
//        {
//          var v156 = &objIt.results[objIt.resultCount - 1];
//          v156.flags |= 2;
//        }
//
//        if (v233 & 0x4000000)
//        {
//          var v157 = &objIt.results[objIt.resultCount - 1];
//          v157.flags |= 4;
//        }
//
//        var v158 = objIt.flags;
//        if ((v158 & 0x200))
//        {
//          var v159 = &objIt.results[objIt.resultCount];
//          GetLocAndOffsetsFromOverallOffsets /*0x10028fb0*/(
//            v239,
//            v240,
//            (LocXY*) &v159[-1].intersectionPoint,
//            &v159[-1].intersectionPoint.off_x,
//            &v159[-1].intersectionPoint.off_y);
//          *((float*) &objIt.results[objIt.resultCount] - 2) = v241;
//        }
//
//        if ((v160 = objIt.flags, objIt.flags & 0x10) && v235 || v160 & 0x20 && v233 & 0x4000000)
//        {
//          var v228 = v236;
//          var v229 = 0;
//          if (*(int*) v236 > 0)
//          {
//            var v230 = (int) (v236 + 40);
//            do
//            {
//              if (v257[v229])
//              {
//                var v231 = *(int*) (v230 + 4);
//                ulong v232 = *(_QWORD*) v230;
//                GameSystems.MapSector.UnlockSector(*(_QWORD*) v230);
//                v228 = v236;
//              }
//
//              ++v229;
//              v230 += 8;
//            } while (v229 < *(int*) v228);
//          }
//
//          return objIt.Count;
//        }
//      }
//    }
//
//    if (objIt.flags & 0x40 && objIt.resultCount > 0)
//    {
//      var v161 = (string) &objIt.results.loc;
//      a2.location.locx = *(int*) v161;
//      a2.location.locy = *((int*) v161 + 1);
//      a2.off_x = *((float*) v161 + 2);
//      a2.off_y = *((float*) v161 + 3);
//      v162 = 64;
//      v246 = 64;
//    }
//    else
//    {
//      v162 = v249;
//    }
//
//    v249 = v162 + 1;
//    if (v162 + 1 > v248)
//    {
//      *(float*) &v39 = overallOffX;
//      v37 = overallOffY;
//      v38 = v248;
//      goto LABEL_193;
//    }
//  }
//
//  var v223 = v236;
//  var v224 = 0;
//  if (*(int*) v236 <= 0)
//  {
//    return objIt.Count;
//  }
//
//  var v225 = (int) (v236 + 40);
//  do
//  {
//    if (v257[v224])
//    {
//      var v226 = *(int*) (v225 + 4);
//      ulong v227 = *(_QWORD*) v225;
//      GameSystems.MapSector.UnlockSector(*(_QWORD*) v225);
//      v223 = v236;
//    }
//
//    ++v224;
//    v225 += 8;
//  } while (v224 < *(int*) v223);
//
//  result = objIt.resultCount;

//  return result;
}

private static bool IsStuff(GameObjectBody v169, RaycastPacket objIt, ref LocAndOffsets a2)
{
  var v178 = v169.DistanceToLocInFeet(objIt.origin);
  var v243 = v178 * locXY.INCH_PER_FEET;
  var v181 = v169.GetRadius();
  v243 = v243 - v181;
  var v182 = objIt.origin.DistanceTo(a2);
  return v182 >= v243;
}

private class TLESub
{
  public int Nx;
  public int field_4;
  public locXY[] tileLocs = new locXY[4];
  public SectorLoc[] sectorLocs = new SectorLoc[4];
  public int[] sectorTileCoords = new int[4];
  public int[] deltas = new int[4];
  public int field_68;
  public int field_6C;
}

private class TileListEntry
{
  public int Ny;
  public int field_4;
  public TLESub[] tlesub = new TLESub[4];

  public TileListEntry()
  {
    for (var i = 0; i < tlesub.Length; i++)
    {
      tlesub[i] = new TLESub();
    }
  }
}

[TempleDllLocation(0x100824d0)]
private static unsafe bool BuildTileListFromRect(TileRect xyRect, out TileListEntry tle)
{
  int xSize;
  int xSizeMin1;
  int ySize;
  int *v6;
  int v8;
  int deltaY;
  int v10;
  int *v13;
  int v14;
  int v18;
  int v19;
  int v20;
  int* xList = stackalloc int[5];
  int* ylist = stackalloc int[5];

  xSize = build_clamped_tile_coordlist/*0x10081d80*/(xyRect.x1, xyRect.x2 + 1, 64, xList);
  xSizeMin1 = xSize - 1;
  v19 = xSize - 1;
  if (xSize == 1)
  {
    tle = null;
    return false;
  }

  ySize = build_clamped_tile_coordlist /*0x10081d80*/(xyRect.y1, xyRect.y2 + 1, 64, ylist) - 1;
  if ((v20 = ySize) == 0)
  {
    tle = null;
    return false;
  }

  tle = new TileListEntry();
  if (ySize > 0)
  {
    v6 = ylist;
    var v7 = 0;
    // v7 = (char *) tle.tlesub.deltas; // @0x58
    v13 = ylist;
    var v17 = 0;
    // v17 = (char *) tle.tlesub.deltas;  // @0x58
    v18 = ySize;
    do
    {
      v8 = 0;
      deltaY = (int) (*( v6 + 1) - *v6);
      tle.tlesub[v7].Nx = 0;
      tle.tlesub[v7].field_68 = deltaY;
      v14 = 0;
      if (xSizeMin1 > 0)
      {
        v10 = (int) (v7 - 0x30);
        var v15 = 0;
        do
        {
          var LODWORDv11 = xList[v8];
          var HIDWORDv11 = *v6;
          var tileLoc = new locXY(LODWORDv11, HIDWORDv11);
          tle.tlesub[v7].tileLocs[v15] = tileLoc;
          tle.tlesub[v7].sectorLocs[v15] = new SectorLoc(tileLoc);
          var v12 = GetSectorTileCoords(LODWORDv11, HIDWORDv11);
          LODWORDv11 = xList[v14];
          xSizeMin1 = v19;
          tle.tlesub[v7].sectorTileCoords[v15] = v12;
          tle.tlesub[v7].deltas[v15] = (int) (xList[v14 + 1] - LODWORDv11);
          v8 = v14 + 1;
          v10 += 8;
          v15++;
          v6 = v13;
          v14 = v8;
        } while (v8 < v19);

        ySize = v20;
        v7 = v17;
      }

      tle.tlesub[v7].Nx = xSizeMin1;
      ++v6;
      v7++;
      v13 = v6;
      v17 = v7;
      --v18;
    } while (v18 != 0);
  }

  tle.Ny = ySize;
  return true;
}

[TempleDllLocation(0x100ab7f0)]
public static int GetSectorTileCoords(int locx, int locy)
{
  return locx & 0x3F | ((locy & 0x3F) << 6);
}

[TempleDllLocation(0x10081d80)]
private static unsafe int build_clamped_tile_coordlist(int startLoc, int endLoc, int increment, int *locs)
{
  int *v4;
  int nextLoc;

  *locs = startLoc;
  v4 = locs + 1;
  nextLoc = (startLoc / increment + 1) * increment;
  for ( ; nextLoc < endLoc; nextLoc += increment )
  {
    *v4 = nextLoc;
    ++v4;
  }
  *v4 = endLoc;
  return (int) (v4 - locs + 1);
}


    }
}