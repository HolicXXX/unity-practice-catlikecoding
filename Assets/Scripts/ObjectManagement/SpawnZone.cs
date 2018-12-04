using UnityEngine;

namespace ObjectManagement
{
    [System.Serializable]
    public struct FloatRange
    {
        public float min, max;

        public float RandomValueInRange
        {
            get
            {
                return Random.Range(min, max);
            }
        }
    }

    [System.Serializable]
    public struct ColorRangeHSV
    {
        [FloatRangeSlider(0f, 1f)]
        public FloatRange hue, saturation, value;

        public Color RandomInRange
        {
            get
            {
                return Random.ColorHSV(
                        hue.min, hue.max,
                        saturation.min, saturation.max,
                        value.min, value.max,
                        1f, 1f
                    );
            }
        }
    }

    public abstract class SpawnZone : PersistableObject
    {
        [System.Serializable]
        public struct SpawnConfiguration
        {
            public enum MovementDirection
            {
                Forward,
                Upward,
                Outward,
                Random
            }

            public ShapeFactory[] factories;

            public MovementDirection movementDirection;

            public FloatRange speed;

            public FloatRange angularSpeed;

            public FloatRange scale;

            public ColorRangeHSV color;

            public bool uniformColor;
        }

        public abstract Vector3 SpawnPoint { get; }

        [SerializeField]
        SpawnConfiguration spawnConfig;

        public virtual Shape SpawnShape()
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            var tran = shape.transform;
            tran.localPosition = SpawnPoint;
            tran.localRotation = Random.rotation;
            tran.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            if (spawnConfig.uniformColor)
            {
                shape.SetColor(spawnConfig.color.RandomInRange);
            }else
            {
                for(int i = 0;i < shape.ColorCount; ++i)
                {
                    shape.SetColor(spawnConfig.color.RandomInRange, i);
                }
            }
            shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
            var zoneTrans = transform;
            Vector3 direction;
            switch (spawnConfig.movementDirection)
            {
                case SpawnConfiguration.MovementDirection.Upward:
                    direction = zoneTrans.up;
                    break;
                case SpawnConfiguration.MovementDirection.Outward:
                    direction = (zoneTrans.localPosition - zoneTrans.position).normalized;
                    break;
                case SpawnConfiguration.MovementDirection.Random:
                    direction = Random.onUnitSphere;
                    break;
                default:
                    direction = zoneTrans.forward;
                    break;
            }
            shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
            return shape;
        }
    }
}
