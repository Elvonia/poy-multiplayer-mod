using System.IO;
using UnityEngine;

namespace Multiplayer
{
    public class Player
    {
        private PlayerShadow player;
        private Color color;

        private Vector3 originShift;
        private int sceneIndex;

        private Vector3 position;
        private Quaternion rotation;

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

        public Player()
        {
            player = Object.FindObjectOfType<PlayerShadow>();
        }

        public byte[] GetColorBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);

                return stream.ToArray();
            }
        }

        public byte[] GetPositionBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);

                writer.Write(rotation.x);
                writer.Write(rotation.y);
                writer.Write(rotation.z);
                writer.Write(rotation.w);

                writer.Write(leftHandPos.x);
                writer.Write(leftHandPos.y);
                writer.Write(leftHandPos.z);

                writer.Write(rightHandPos.x);
                writer.Write(rightHandPos.y);
                writer.Write(rightHandPos.z);

                writer.Write(leftFootPos.x);
                writer.Write(leftFootPos.y);
                writer.Write(leftFootPos.z);

                writer.Write(rightFootPos.x);
                writer.Write(rightFootPos.y);
                writer.Write(rightFootPos.z);

                writer.Write(leftFootBendPos.x);
                writer.Write(leftFootBendPos.y);
                writer.Write(leftFootBendPos.z);

                writer.Write(rightFootBendPos.x);
                writer.Write(rightFootBendPos.y);
                writer.Write(rightFootBendPos.z);

                writer.Write(leftHandRot.x);
                writer.Write(leftHandRot.y);
                writer.Write(leftHandRot.z);
                writer.Write(leftHandRot.w);

                writer.Write(rightHandRot.x);
                writer.Write(rightHandRot.y);
                writer.Write(rightHandRot.z);
                writer.Write(rightHandRot.w);

                writer.Write(leftFootRot.x);
                writer.Write(leftFootRot.y);
                writer.Write(leftFootRot.z);
                writer.Write(leftFootRot.w);

                writer.Write(rightFootRot.x);
                writer.Write(rightFootRot.y);
                writer.Write(rightFootRot.z);
                writer.Write(rightFootRot.w);

                writer.Write(leftArmStretch);
                writer.Write(rightArmStretch);

                return stream.ToArray();
            }
        }

        public byte[] GetSceneBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(sceneIndex);
                return stream.ToArray();
            }
        }

        public int GetSceneIndex()
        {
            return sceneIndex;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        public void SetScene(int sceneIndex)
        {
            this.sceneIndex = sceneIndex;
        }

        public void UpdatePlayer()
        {
            originShift = Vector3.zero;

            if (OriginShift.singleton != null)
                originShift = OriginShift.LocalOffset.ToVector3();

            position = player.transform.position + originShift;
            rotation = player.transform.rotation;

            leftHandPos = player.handIK_L.solver.arm.target.position + originShift;
            rightHandPos = player.handIK_R.solver.arm.target.position + originShift;

            leftFootPos = player.footIK_L.solver.target.position + originShift;
            rightFootPos = player.footIK_R.solver.target.position + originShift;

            leftFootBendPos = player.realleftKnee.transform.position + originShift;
            rightFootBendPos = player.realrightKnee.transform.position + originShift;

            leftHandRot = player.handIK_L.solver.arm.target.rotation;
            rightHandRot = player.handIK_R.solver.arm.target.rotation;

            leftFootRot = player.footIK_L.solver.target.rotation;
            rightFootRot = player.footIK_R.solver.target.rotation;

            leftArmStretch = player.handIK_L.solver.arm.armLengthMlp;
            rightArmStretch = player.handIK_R.solver.arm.armLengthMlp;
        }
    }
}