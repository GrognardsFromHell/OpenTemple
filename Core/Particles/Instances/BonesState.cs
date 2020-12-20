using System;
using System.Numerics;

namespace OpenTemple.Core.Particles.Instances
{
    public class BonesState
    {
        private static readonly Random Random = new Random();
        private readonly int _boneCount;
        private readonly int _childBoneCount;
        private readonly float[] _distFromParent;
        private readonly float _distFromParentSum;

        private readonly IPartSysExternal _external;
        private readonly int[] _ids;

        private readonly object _object;
        private readonly int[] _parentIds;
        private readonly Vector3[] _pos;
        private readonly Vector3[] _prevPos;

        public BonesState(IPartSysExternal external, object attachedTo)
        {
            _object = attachedTo;
            _external = external;

            var boneCount = external.GetBoneCount(attachedTo);

            if (boneCount > 0)
            {
                _ids = new int[boneCount];
                _parentIds = new int[boneCount];
                _distFromParent = new float[boneCount];
                _pos = new Vector3[boneCount];
                _prevPos = new Vector3[boneCount];

                for (var boneId = 0; boneId < boneCount; ++boneId)
                {
                    var parentId = external.GetParentChildBonePos(
                        attachedTo,
                        boneId,
                        out var parentPos,
                        out var bonePos
                    );
                    if (parentId >= 0)
                    {
                        var length = (bonePos - parentPos).Length();

                        _ids[_childBoneCount] = boneId;
                        _parentIds[_childBoneCount] = parentId;
                        _distFromParent[_childBoneCount] = length;
                        _distFromParentSum += length;
                        _childBoneCount++;
                    }
                }

                _boneCount = boneCount;
            }

            UpdatePos();
        }

        public void UpdatePos()
        {
            for (var i = 0; i < _boneCount; ++i)
            {
                // Update the position of each bone, but store the previous position
                _prevPos[i] = _pos[i];
                _external.GetBonePos(_object, i, out _pos[i]);
            }
        }

        public bool GetRandomPos(float timeScale, out Vector3 result)
        {
            if (_childBoneCount == 0)
            {
                result = default;
                return false;
            }

            /*
                ToEE uses a specific particle system specific randomness sequence here,
                but since this is purely graphical, we will not.
            */
            var randFactor = (float) Random.NextDouble();
            var randDist = randFactor * _distFromParentSum;

            /*
                Instead of just randomly selecting a bone, each bone is
                effectively weighted by the distance between its parent
                and the bone. This makes sense, since a bigger space means
                it's probably a bigger bone and it shoud be used more often
                to spawn particles.
            */
            int i;
            for (i = 0; i < _childBoneCount; ++i)
            {
                if (randDist < _distFromParent[i])
                {
                    break;
                }

                randDist -= _distFromParent[i];
            }

            // If we couldn't find any (how does this happen?)
            // use the last  one
            if (i == _childBoneCount)
            {
                i = _childBoneCount - 1;
            }

            var boneId = _ids[i];
            var boneParentId = _parentIds[i];

            if (boneParentId >= 0)
            {
                var distFactor = randDist / _distFromParent[i];
                if (distFactor > 1 || distFactor < 0)
                {
                    distFactor = 0.5f;
                }

                var bonePos = _pos[boneId];
                var parentPos = _pos[boneParentId];
                var posNow = parentPos + (bonePos - parentPos) * distFactor;

                var bonePrevPos = _prevPos[boneId];
                var parentPrevPos = _prevPos[boneParentId];
                var posPrev = parentPrevPos + (bonePrevPos - parentPrevPos) * distFactor;

                // Lerp between the last simulation point and now using the time factor
                result = Vector3.Lerp(posPrev, posNow, timeScale);
                return true;
            }

            result = default;
            return false;
        }
    }
}