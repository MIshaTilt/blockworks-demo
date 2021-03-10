using System;
using System.Collections.Generic;
using System.Linq;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public static class ShadowExtensions
    {
        public static (Vector3 position, Quaternion rotation, bool valid) AlignShadow(this SnapPreview shadow, Chunk chunkSource)
        {
            var connections = chunkSource.GetConnections();
            connections = FilterOutCollinear(connections);

            // Chose two closes connections and choose origin and alignment.
            // If only one connection is available use that one.
            if (connections.Length == 0)
            {
                return (default, default, false);
            }

            if (connections.Length == 1)
            {
                var thisSocket = connections[0].thisSocket;
                var otherSocket = connections[0].otherSocket;

                var (position1, rotation1) = shadow.AlignShadowSingle(thisSocket, otherSocket, chunkSource.transform);
                return (position1, rotation1, true);
            }

            var thisSocketA = connections[0].thisSocket;
            var otherSocketA = connections[0].otherSocket;
            var thisSocketB = connections[1].thisSocket;
            var otherSocketB = connections[1].otherSocket;

            var (position, rotation) =  shadow.AlignShadow(thisSocketA, thisSocketB, otherSocketA, otherSocketB, chunkSource.transform);
            return (position, rotation, true);
        }

        private static (Transform thisSocket, Transform otherSocket)[] FilterOutCollinear((Transform thisSocket, Transform otherSocket)[] connections)
        {
            var output = new List<(Transform thisSocket, Transform otherSocket)>();
            foreach (var connection1 in connections)
            {
                var isCollinear = false;
                foreach (var connection2 in connections)
                {
                    if (connection1 != connection2)
                    {
                        // Chose two closes connections and choose origin and alignment.
                        var thisSocketA = connection1.thisSocket;
                        var otherSocketA = connection1.otherSocket;
                        var thisSocketB = connection2.thisSocket;
                        var otherSocketB = connection2.otherSocket;
            
                        var directionA = otherSocketA.up;
                        var directionB = otherSocketA.position - otherSocketB.position;

                        // Check if the directions are collinear
                        if (Math.Abs(directionA.Dot(directionB)) > 0.0001f)
                        {
                            isCollinear = true;
                        }
                    }
                }

                if (isCollinear == false)
                {
                    output.Add(connection1);
                }
            }

            return output.ToArray();
        }

        private static (Vector3 position, Quaternion rotation) AlignShadowSingle(this SnapPreview shadow, Transform thisA, Transform otherA, Transform blockSource)
        {
            var possibleDirs = new[] {
                (0, otherA.forward.normalized),
                (1, -otherA.forward.normalized),
                (2, otherA.right.normalized),
                (3, -otherA.right.normalized)
            };

            var closestDirection = possibleDirs.OrderByDescending(p => p.Item2.Angle(thisA.right.normalized)).First().Item1;
            
            var thisDir = thisA.right.normalized;
            var otherDir = otherA.right.normalized;

            switch (closestDirection)
            {
                case 0:
                    otherDir = otherA.forward.normalized;
                    break;
                case 1:
                    otherDir = -otherA.forward.normalized;
                    break;
                case 2:
                    otherDir = otherA.right.normalized;
                    break;
                case 3:
                    otherDir = -otherA.right.normalized;
                    break;
            }

            // TODO Direction has to be inverted? Maybe its because the different default direction of female/male socket
            otherDir = -otherDir;
            
            return shadow.AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }

        private static (Vector3 position, Quaternion rotation) AlignShadow(this SnapPreview shadow, Transform thisA, Transform thisB, Transform otherA, Transform otherB, Transform blockSource)
        {
            var thisDir = (thisB.position - thisA.position).normalized;
            var otherDir = (otherB.position - otherA.position).normalized;

            return shadow.AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }
        
        private static (Vector3 position, Quaternion rotation) AlignShadow(this SnapPreview shadow, Transform thisA, Vector3 thisDir, Transform otherA, Vector3 otherDir, Transform blockSource)
        {
            const bool OLDWAY = false;
            
            var thisToOtherRotation = Quaternion.FromToRotation(thisDir, otherDir);

            // Correct for rotation along the direction (multiple valid states for resulting rotation)
            var correctedUpVector = thisToOtherRotation * -thisA.up;
            var angle = Vector3.SignedAngle(correctedUpVector, otherA.up, otherDir);
            var correction = Quaternion.AngleAxis(angle, otherDir);

            if (OLDWAY)
            {
                shadow.transform.rotation = correction * thisToOtherRotation * blockSource.rotation;
                
                shadow.transform.position = blockSource.position;

                // TODO Get the resulting position and rotation without touching the transforms
                var blockSocketLocalPosition = blockSource.InverseTransformPoint(thisA.position);
                var adjustedWorldPosition = shadow.transform.TransformPoint(blockSocketLocalPosition);
                var offset = otherA.position - adjustedWorldPosition;
            
                return (blockSource.position + offset, shadow.transform.rotation);
            }
            else
            {
                var targetRotation = correction * thisToOtherRotation * blockSource.rotation;

                var blockSocketLocalPosition = blockSource.InverseTransformPoint(thisA.position);
                var shadowSocketWorldPosition = shadow.transform.TransformPoint(blockSocketLocalPosition);
                var adjustedWorldPosition = shadowSocketWorldPosition - shadow.transform.position;
                var targetPosition = otherA.position - adjustedWorldPosition;
            
                return (targetPosition, targetRotation);
            }
        }
    }
}