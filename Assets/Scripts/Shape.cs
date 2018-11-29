using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class Shape : PersistableObject {

        [SerializeField]
        MeshRenderer[] meshRenderers;

        public Vector3 AngularVelocity { get; set; }

        public Vector3 Velocity { get; set; }

        private int shapeId = int.MinValue;
        public int ShapeId
        {
            get { return this.shapeId; }
            set
            {
                if(shapeId == int.MinValue && value != int.MinValue)
                {
                    shapeId = value;
                }
            }
        }

        public int MaterialId
        {
            get;
            private set;
        }

        Color[] colors;

        public int ColorCount
        {
            get
            {
                return colors.Length;
            }
        }

        ShapeFactory originFactory;
        public ShapeFactory OriginFactory
        {
            get
            {
                return originFactory;
            }
            set
            {
                if(originFactory == null)
                {
                    originFactory = value;
                }else
                {
                    Debug.LogError("Not Allowed to change origin factory");
                }
            }
        }

        static int colorPropertyId = Shader.PropertyToID("_Color");
        static MaterialPropertyBlock sharedPropertyBlock;

        private void Awake()
        {
            colors = new Color[meshRenderers.Length];
        }

        public void SetMaterial(Material material, int materialId)
        {
            for(int i = 0;i < meshRenderers.Length; ++i)
            {
                meshRenderers[i].material = material;
            }
            MaterialId = materialId;
        }

        public void SetColor(Color color)
        {
            if(sharedPropertyBlock == null)
            {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            for (int i = 0; i < meshRenderers.Length; ++i)
            {
                colors[i] = color;
                meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
            }
        }

        public void SetColor(Color color, int index)
        {
            if (sharedPropertyBlock == null)
            {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            colors[index] = color;
            meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
        }

        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            writer.Write(ColorCount);
            for(int i = 0;i < ColorCount; ++i)
            {
                writer.Write(colors[i]);
            }
            writer.Write(AngularVelocity);
            writer.Write(Velocity);
        }

        public override void Load(GameDataReader reader)
        {
            base.Load(reader);
            if(reader.Version >= 5)
            {
                LoadColor(reader);
            }else
            {
                SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
            }
            AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
            Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        }

        void LoadColor(GameDataReader reader)
        {
            int count = reader.ReadInt();
            int max = count <= colors.Length ? count : colors.Length;
            int i = 0;
            for (; i < max; ++i)
            {
                SetColor(reader.ReadColor(), i);
            }
            if (count > max)
            {
                for (; i < count; i++)
                {
                    reader.ReadColor();
                }
            }
            else if (count < max)
            {
                for (; i < max; i++)
                {
                    SetColor(Color.white, i);
                }
            }
        }

        public void GameUpdate()
        {
            transform.Rotate(AngularVelocity * Time.deltaTime);
            transform.localPosition += Velocity * Time.deltaTime;
        }

        public void Recycle()
        {
            originFactory.Reclaim(this);
        }
    }
}
