using RootMotion.FinalIK;
using Steamworks;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer
{
    public class PlayerClone
    {
        private CSteamID steamID;
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

        private Vector3 leftHandPos;
        private Vector3 rightHandPos;
        private Vector3 leftFootPos;
        private Vector3 rightFootPos;
        private Vector3 leftFootBendPos;
        private Vector3 rightFootBendPos;

        private Quaternion leftHandRot;
        private Quaternion rightHandRot;
        private Quaternion leftFootRot;
        private Quaternion rightFootRot;

        private float leftArmStretch = 1f;
        private float rightArmStretch = 1f;

        private Color color;
        private int sceneIndex;
        private Vector3 originShift;

        private string playerName;
        private GameObject nameTag;
        private TextMesh textMesh;


        public PlayerClone(CSteamID steamID, GameObject shadow)
        {
            this.steamID = steamID;
            this.playerName = SteamFriends.GetFriendPersonaName(steamID);

            color = Color.white;

            if (shadow != null)
                CreatePlayerGameObject(shadow);
        }

        public void DestroyPlayerGameObject()
        {
            if (player != null)
            {
                Object.Destroy(player);
                player = null;
            }
        }

        private void CreateNameTag()
        {
            nameTag = new GameObject("NameTag");
            textMesh = nameTag.AddComponent<TextMesh>();

            textMesh.text = playerName;
            textMesh.fontSize = 56;
            textMesh.characterSize = 0.01f;
            textMesh.color = color;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            nameTag.transform.SetParent(player.transform);
            nameTag.transform.localPosition = new Vector3(0, 0.80f, 0);

            nameTag.AddComponent<LookAtPlayer>();
        }

        public void CreatePlayerGameObject(GameObject shadow)
        {
            if (player == null)
            {
                player = Object.Instantiate(shadow);
                player.name = $"PlayerClone_{steamID}";
                player.SetActive(true);

                position = shadow.transform.position;
                rotation = shadow.transform.rotation;

                CreateNameTag();

                leftHand = new GameObject("LeftHand");
                leftHand.transform.SetParent(player.transform);

                rightHand = new GameObject("RightHand");
                rightHand.transform.SetParent(player.transform);

                leftFoot = new GameObject("LeftFoot");
                leftFoot.transform.SetParent(player.transform);

                rightFoot = new GameObject("RightFoot");
                rightFoot.transform.SetParent(player.transform);

                leftFootBend = new GameObject("LeftFootBend");
                leftFootBend.transform.SetParent(player.transform);

                rightFootBend = new GameObject("RightFootBend");
                rightFootBend.transform.SetParent(player.transform);

                leftHandIK = player.transform.GetChild(6).GetComponent<ArmIK>();
                rightHandIK = player.transform.GetChild(7).GetComponent<ArmIK>();

                leftFootIK = player.transform.GetChild(4).GetComponent<LimbIK>();
                rightFootIK = player.transform.GetChild(5).GetComponent<LimbIK>();

                leftHandIK.solver.arm.target = leftHand.transform;
                rightHandIK.solver.arm.target = rightHand.transform;

                leftFootIK.solver.target = leftFoot.transform;
                rightFootIK.solver.target = rightFoot.transform;

                leftFootIK.solver.bendGoal = leftFootBend.transform;
                rightFootIK.solver.bendGoal = rightFootBend.transform;

                leftHandIK.solver.arm.armLengthMlp = leftArmStretch;
                rightHandIK.solver.arm.armLengthMlp = rightArmStretch;

                leftHandIK.fixTransforms = true;
                rightHandIK.fixTransforms = true;
            }
        }

        public GameObject GetPlayer()
        {
            return player;
        }

        public int GetSceneIndex()
        {
            return sceneIndex;
        }

        public CSteamID GetSteamID()
        {
            return steamID;
        }

        public void SetPositionDataFromBytes(byte[] data)
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

                leftFootBendPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightFootBendPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftHandRot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightHandRot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftFootRot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                rightFootRot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                leftArmStretch = reader.ReadSingle();
                rightArmStretch = reader.ReadSingle();
            }
        }

        public void SetColor(Color color)
        {
            this.color = color;
            textMesh.color = color;

            SkinnedMeshRenderer[] meshRenderers = player.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in meshRenderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = color;
            }
        }

        public void SetSceneIndex(int sceneIndex)
        {
            this.sceneIndex = sceneIndex;
        }

        public void UpdateTransforms()
        {
            originShift = Vector3.zero;

            if (OriginShift.singleton != null)
                originShift = OriginShift.LocalOffset.ToVector3();

            player.transform.position = position - originShift;
            player.transform.rotation = rotation;

            leftHand.transform.position = leftHandPos - originShift;
            rightHand.transform.position = rightHandPos - originShift;

            leftFoot.transform.position = leftFootPos - originShift;
            rightFoot.transform.position = rightFootPos - originShift;

            leftFootBend.transform.position = leftFootBendPos - originShift;
            rightFootBend.transform.position = rightFootBendPos - originShift;

            leftHand.transform.rotation = leftHandRot;
            rightHand.transform.rotation = rightHandRot;

            leftFoot.transform.rotation = leftFootRot;
            rightFoot.transform.rotation = rightFootRot;

            leftHandIK.solver.arm.armLengthMlp = leftArmStretch;
            rightHandIK.solver.arm.armLengthMlp = rightArmStretch;

            SetColor(color);
        }
    }
}