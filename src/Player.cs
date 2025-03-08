using MelonLoader;
using System.IO;
using UnityEngine;

namespace CoopMod
{
    public class Player
    {
        private GameObject player;
        private PlayerShadow shadow;

        private Vector3 position;
        private Quaternion rotation;

        private Vector3 leftHandPos = Vector3.zero;
        private Vector3 rightHandPos = Vector3.zero;
        private Vector3 leftFootPos = Vector3.zero;
        private Vector3 rightFootPos = Vector3.zero;
        private Vector3 leftFootBendPos = Vector3.zero;
        private Vector3 rightFootBendPos = Vector3.zero;

        private Quaternion leftHandRot = Quaternion.identity;
        private Quaternion rightHandRot = Quaternion.identity;
        private Quaternion leftFootRot = Quaternion.identity;
        private Quaternion rightFootRot = Quaternion.identity;

        private float leftArmStretch = 1f;
        private float rightArmStretch = 1f;

        public Player()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            shadow = GameObject.FindObjectOfType<PlayerShadow>();
        }

        public byte[] GetPlayerDataBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
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

        public void UpdatePlayer()
        {
            position = shadow.transform.position;
            rotation = shadow.transform.rotation;

            leftHandPos = shadow.handIK_L.solver.arm.target.position;
            rightHandPos = shadow.handIK_R.solver.arm.target.position;

            leftFootPos = shadow.footIK_L.solver.target.position;
            rightFootPos = shadow.footIK_R.solver.target.position;

            leftFootBendPos = shadow.realleftKnee.transform.position;
            rightFootBendPos = shadow.realrightKnee.transform.position;

            leftHandRot = shadow.handIK_L.solver.arm.target.rotation;
            rightHandRot = shadow.handIK_R.solver.arm.target.rotation;

            leftFootRot = shadow.footIK_L.solver.target.rotation;
            rightFootRot = shadow.footIK_R.solver.target.rotation;

            leftArmStretch = shadow.handIK_L.solver.arm.armLengthMlp;
            rightArmStretch = shadow.handIK_R.solver.arm.armLengthMlp;

        }
    }
}