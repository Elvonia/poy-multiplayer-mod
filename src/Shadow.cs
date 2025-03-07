using Steamworks;
using System.IO;
using UnityEngine;

namespace CoopMod
{
    public class Shadow
    {
        private CSteamID steamID;

        private GameObject shadow;

        private Vector3 position;
        private Quaternion rotation;

        private Vector3 leftHand;
        private Vector3 rightHand;

        private Vector3 leftKnee;
        private Vector3 rightKnee;

        private Vector3 leftFoot;
        private Vector3 rightFoot;

        private float leftArm;
        private float rightArm;

        public Shadow(CSteamID steamID, GameObject shadow)
        {
            this.steamID = steamID;
            this.shadow = shadow;

            position = Vector3.zero;
            rotation = Quaternion.identity;

            leftHand = Vector3.zero;
            rightHand = Vector3.zero;

            leftKnee = Vector3.zero;
            rightKnee = Vector3.zero;

            leftFoot = Vector3.zero;
            rightFoot = Vector3.zero;

            leftArm = 0f;
            rightArm = 0f;
        }

        public void SetShadowDataFromBytes(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftHand = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightHand = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftKnee = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightKnee = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftFoot = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightFoot = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftArm = reader.ReadSingle();
                rightArm = reader.ReadSingle();
            }
        }

        public void SetShadowMaterial(Color color)
        {
            SkinnedMeshRenderer[] meshRenderers = shadow.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer renderer in meshRenderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = color;
            }
        }
    }
}