using System.Drawing;

namespace ParticleGame
{
    public static class Power
    {
        public delegate bool PowerCondition(IEnumerable<ParticleTypes.Types> poweredInputs);

        /// <summary>
        /// Contains particle types which emit power to adjacent particles.
        /// </summary>
        public static readonly HashSet<ParticleTypes.Types> EmitsPower = new()
        {
            ParticleTypes.Types.Battery, ParticleTypes.Types.Inverter
        };

        /// <summary>
        /// Whether electrical power should spread through particles of this type.
        /// Note: power always spreads through particles of the same type.
        /// </summary>
        public static readonly HashSet<ParticleTypes.Types> ConductsPower = new()
        {
            ParticleTypes.Types.Water, ParticleTypes.Types.WaterPowered, ParticleTypes.Types.Wire, ParticleTypes.Types.WirePowered,
            ParticleTypes.Types.Copper, ParticleTypes.Types.CopperPowered, ParticleTypes.Types.Steel, ParticleTypes.Types.SteelPowered
        };

        /// <summary>
        /// Whether only un-conditional electrical power should spread through particles of this type.
        /// Contained types should also be in the <see cref="ConductsPower"/> set.
        /// Note: power always spreads through particles of the same type.
        /// </summary>
        public static readonly HashSet<ParticleTypes.Types> ConductsUnconditionalPower = new()
        {
            ParticleTypes.Types.Copper, ParticleTypes.Types.CopperPowered
        };

        /// <summary>
        /// Whether this type should not conduct power to emitters.
        /// Contained types should also be in the <see cref="ConductsPower"/> set.
        /// </summary>
        public static readonly HashSet<ParticleTypes.Types> WillNotPowerEmitters = new()
        {
            ParticleTypes.Types.Steel, ParticleTypes.Types.SteelPowered
        };

        /// <summary>
        /// Maps unpowered particle types to their powered counterparts (if applicable).
        /// </summary>
        public static readonly Dictionary<ParticleTypes.Types, ParticleTypes.Types> PoweredStates = new()
        {
            { ParticleTypes.Types.Water, ParticleTypes.Types.WaterPowered },
            { ParticleTypes.Types.Wire, ParticleTypes.Types.WirePowered },
            { ParticleTypes.Types.Light, ParticleTypes.Types.LightPowered },
            { ParticleTypes.Types.Tap, ParticleTypes.Types.TapPowered },
            { ParticleTypes.Types.Inverter, ParticleTypes.Types.InverterPowered },
            { ParticleTypes.Types.Copper, ParticleTypes.Types.CopperPowered },
            { ParticleTypes.Types.Steel, ParticleTypes.Types.SteelPowered },
        };

        /// <summary>
        /// Maps powered particle types to their unpowered counterparts (if applicable).
        /// </summary>
        public static readonly Dictionary<ParticleTypes.Types, ParticleTypes.Types> UnpoweredStates = new()
        {
            { ParticleTypes.Types.WaterPowered, ParticleTypes.Types.Water },
            { ParticleTypes.Types.WirePowered, ParticleTypes.Types.Wire },
            { ParticleTypes.Types.LightPowered, ParticleTypes.Types.Light },
            { ParticleTypes.Types.TapPowered, ParticleTypes.Types.Tap },
            { ParticleTypes.Types.InverterPowered, ParticleTypes.Types.Inverter },
            { ParticleTypes.Types.CopperPowered, ParticleTypes.Types.Copper },
            { ParticleTypes.Types.SteelPowered, ParticleTypes.Types.Steel },
        };

        public static bool InverterEmitCondition(IEnumerable<ParticleTypes.Types> poweredInputs)
        {
            return !poweredInputs.Any();
        }

        public static readonly Dictionary<ParticleTypes.Types, PowerCondition> EmitterConditions = new()
        {
            { ParticleTypes.Types.Inverter, new(InverterEmitCondition) }
        };

        // Predefining hash set prevents excessive heap allocations.
        private static readonly HashSet<Point> seenPoints = new(500 * 500);
        private static readonly Queue<(Point, int)> pointQueue = new(500 * 500);
        private static readonly Queue<Point> bundleQueue = new(500 * 500);

        // Last emitter bundles are cached
        private static DateTime emitterBundleCacheTime = DateTime.MinValue;
        private static readonly Dictionary<Point, int> emitterBundleIDs = new(500 * 500);
        private static readonly Dictionary<int, HashSet<ParticleTypes.Types>> emitterBundleInputs = new(500 * 500);

        public static void UpdateFieldPower(ParticleField field)
        {
            seenPoints.Clear();
            pointQueue.Clear();

            // Unpower all particles first
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType == ParticleTypes.Types.Air)
                    {
                        continue;
                    }
                    ParticleTypes.Types particleType = data.ParticleType;
                    data.ParticleType = UnpoweredStates.GetValueOrDefault(particleType, particleType);
                    if (particleType != data.ParticleType)
                    {
                        // Particle type changed (i.e. became unpowered), update color to draw
                        field.UpdatePoint(x, y);
                    }
                }
            }

            if (emitterBundleCacheTime < field.LastChange.Time
                && (field.LastChange.ParticleType == ParticleTypes.Types.Air || EmitsPower.Contains(field.LastChange.ParticleType)))
            {
                // Emitter bundle cache is out of date, update it,
                // but only if an emitter has been added or a particle has been erased
                bundleQueue.Clear();
                emitterBundleIDs.Clear();
                emitterBundleInputs.Clear();
                emitterBundleCacheTime = field.LastChange.Time;

                int currentEmitterBundle = 0;
                for (int x = 0; x < 500; x++)
                {
                    for (int y = 0; y < 500; y++)
                    {
                        ParticleData data = field[x, y];
                        Point srcPoint = new(x, y);
                        if (data.ParticleType == ParticleTypes.Types.Air || !EmitterConditions.ContainsKey(data.ParticleType))
                        {
                            continue;
                        }
                        if (!seenPoints.Add(srcPoint))
                        {
                            continue;
                        }
                        bundleQueue.Enqueue(srcPoint);

                        while (bundleQueue.TryDequeue(out Point point))
                        {
                            if (point != srcPoint && !seenPoints.Add(point))
                            {
                                continue;
                            }

                            ParticleData adjData = field[point.X, point.Y];
                            if (adjData.ParticleType == data.ParticleType)
                            {
                                emitterBundleIDs[point] = currentEmitterBundle;
                                foreach (Point adj in ParticleGame.Adjacent)
                                {
                                    Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                                    if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                                    {
                                        continue;
                                    }
                                    bundleQueue.Enqueue(newTarget);
                                }
                            }
                        }
                        emitterBundleInputs[currentEmitterBundle] = new HashSet<ParticleTypes.Types>();
                        currentEmitterBundle++;
                    }
                }

                seenPoints.Clear();
                pointQueue.Clear();
            }

            // From any non-conditional power emitters, power connected particles
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType != ParticleTypes.Types.Air
                        && EmitsPower.Contains(data.ParticleType)
                        && !EmitterConditions.ContainsKey(data.ParticleType))
                    {
                        Point point = new(x, y);
                        // Add all points around power emitter to queue
                        foreach (Point adj in ParticleGame.Adjacent)
                        {
                            Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                            if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                            {
                                continue;
                            }
                            ParticleTypes.Types particleType = field[x, y].ParticleType;
                            // Emitters cannot directly power themselves
                            if (particleType != field[newTarget.X, newTarget.Y].ParticleType)
                            {
                                pointQueue.Enqueue((newTarget, -1));
                            }
                            else
                            {
                                _ = seenPoints.Add(newTarget);
                            }
                        }
                    }
                }
            }
            TransmitPower(field);

            // Second pass for conditional power emitters
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    Point point = new(x, y);
                    if (data.ParticleType != ParticleTypes.Types.Air
                        && EmitterConditions.ContainsKey(data.ParticleType)
                        && EmitterConditions[data.ParticleType](emitterBundleInputs[emitterBundleIDs[point]]))
                    {
                        int bundleID = emitterBundleIDs[point];
                        // Add all points around power emitter to queue
                        foreach (Point adj in ParticleGame.Adjacent)
                        {
                            Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                            if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                            {
                                continue;
                            }
                            ParticleTypes.Types particleType = field[x, y].ParticleType;
                            // Emitters cannot directly power themselves
                            if (particleType != field[newTarget.X, newTarget.Y].ParticleType)
                            {
                                pointQueue.Enqueue((newTarget, bundleID));
                            }
                            else
                            {
                                _ = seenPoints.Add(newTarget);
                            }
                        }
                    }
                }
            }
            TransmitPower(field);
        }

        private static void TransmitPower(ParticleField field)
        {
            while (pointQueue.TryDequeue(out (Point, int) value))
            {
                Point powerPoint = value.Item1;
                int bundleID = value.Item2;
                ParticleData currentData = field[powerPoint.X, powerPoint.Y];
                if (currentData.ParticleType == ParticleTypes.Types.Air)
                {
                    continue;
                }

                ParticleTypes.Types particleType = currentData.ParticleType;

                // Power source is conditional, but this particle only conducts non-conditional power
                if (bundleID >= 0 && ConductsUnconditionalPower.Contains(particleType))
                {
                    continue;
                }

                // If this particle has already been processed, move on
                if (!seenPoints.Add(powerPoint))
                {
                    continue;
                }

                currentData.ParticleType = PoweredStates.GetValueOrDefault(currentData.ParticleType, currentData.ParticleType);
                if (currentData.ParticleType != particleType)
                {
                    // Particle type changed (i.e. became powered), update color to draw
                    field.UpdatePoint(powerPoint.X, powerPoint.Y);
                }

                bool isConductive = ConductsPower.Contains(currentData.ParticleType);
                foreach (Point adj in ParticleGame.Adjacent)
                {
                    Point newTarget = new(powerPoint.X + adj.X, powerPoint.Y + adj.Y);
                    if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                    {
                        continue;
                    }
                    // Add a input source to any adjacent bundles
                    if (emitterBundleIDs.ContainsKey(newTarget))
                    {
                        _ = emitterBundleInputs[emitterBundleIDs[newTarget]].Add(currentData.ParticleType);
                    }
                    // If this particle conducts power, add any surrounding particles to queue,
                    // otherwise only add particles of the same type
                    ParticleTypes.Types newType = field[newTarget.X, newTarget.Y].ParticleType;
                    if ((isConductive || newType == particleType)
                        && (!WillNotPowerEmitters.Contains(particleType) || !EmitsPower.Contains(newType)))
                    {
                        pointQueue.Enqueue((newTarget, bundleID));
                    }
                }
            }
        }
    }
}
