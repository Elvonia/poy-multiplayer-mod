using RootMotion.FinalIK;
using Steamworks;
using System.IO;
using UnityEngine;

namespace CoopMod
{
    public class Shadow
    {
        private CSteamID steamID;

        private GameObject shadow;
        private GameObject player;

        private ArmIK leftHandIK;
        private ArmIK rightHandIK;

        private LimbIK leftFootIK;
        private LimbIK rightFootIK;

        private Vector3 position;
        private Quaternion rotation;

        private GameObject leftHand;
        private GameObject rightHand;
        private GameObject leftFoot;
        private GameObject rightFoot;
        private GameObject leftFootBend;
        private GameObject rightFootBend;

        private Vector3 leftHandPos = Vector3.zero;
        private Vector3 rightHandPos = Vector3.zero;
        private Vector3 leftFootPos = Vector3.zero;
        private Vector3 rightFootPos = Vector3.zero;
        private Vector3 leftFootBendPos = Vector3.zero;
        private Vector3 rightFootBendPos = Vector3.zero;

        private float leftArmStretch;
        private float rightArmStretch;


        public Shadow(CSteamID steamID, GameObject shadow)
        {
            this.steamID = steamID;
            this.shadow = shadow;

            player = new GameObject("MP_Player");
            Object.Instantiate(player);

            position = shadow.transform.position;
            rotation = shadow.transform.rotation;

            leftHand = new GameObject("LeftHand");
            rightHand = new GameObject("RightHand");

            leftFoot = new GameObject("LeftFoot");
            rightFoot = new GameObject("RightFoot");

            leftFootBend = new GameObject("LeftFootBend");
            rightFootBend = new GameObject("RightFootBend");

            leftHandIK.solver.arm.target = leftHand.transform;
            rightHandIK.solver.arm.target = rightHand.transform;

            leftFootIK.solver.target = leftFoot.transform;
            rightFootIK.solver.target = rightFoot.transform;

            leftFootIK.solver.bendGoal = leftFootBend.transform;
            rightFootIK.solver.bendGoal = rightFootBend.transform;

            leftHandIK.fixTransforms = true;
            rightHandIK.fixTransforms = true;
        }

        public CSteamID GetSteamID()
        {
            return steamID;
        }

        public void SetShadowDataFromBytes(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftHandPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightHandPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftFootPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightFootPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftArmStretch = reader.ReadSingle();
                rightArmStretch = reader.ReadSingle();
            }

            shadow.transform.position = position;
            shadow.transform.rotation = rotation;
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

        public void UpdateShadowTransforms()
        {
            shadow.transform.position = Vector3.Lerp(shadow.transform.position, position, 0.5f);
            shadow.transform.rotation = Quaternion.Lerp(shadow.transform.rotation, rotation, 0.5f);

            leftHand.transform.position = Vector3.Lerp(leftHand.transform.position, leftHandPos, 0.5f);
            rightHand.transform.position = Vector3.Lerp(rightHand.transform.position, rightHandPos, 0.5f);

            leftFoot.transform.position = Vector3.Lerp(leftFoot.transform.position, leftFootPos, 0.5f);
            rightFoot.transform.position = Vector3.Lerp(rightFoot.transform.position, rightFootPos, 0.5f);

            leftFootBend.transform.position = Vector3.Lerp(leftFootBend.transform.position, leftFootBendPos, 0.5f);
            rightFootBend.transform.position = Vector3.Lerp(rightFootBend.transform.position, rightFootBendPos, 0.5f);
        }
    }
}