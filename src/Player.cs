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

        private Vector3 leftHand;
        private Vector3 rightHand;

        private Vector3 leftKnee;
        private Vector3 rightKnee;

        private Vector3 leftFoot;
        private Vector3 rightFoot;

        private float leftArm;
        private float rightArm;


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

                writer.Write(leftHand.x);
                writer.Write(leftHand.y);
                writer.Write(leftHand.z);

                writer.Write(rightHand.x);
                writer.Write(rightHand.y);
                writer.Write(rightHand.z);

                writer.Write(leftKnee.x);
                writer.Write(leftKnee.y);
                writer.Write(leftKnee.z);

                writer.Write(rightKnee.x);
                writer.Write(rightKnee.y);
                writer.Write(rightKnee.z);

                writer.Write(leftFoot.x);
                writer.Write(leftFoot.y);
                writer.Write(leftFoot.z);

                writer.Write(rightFoot.x);
                writer.Write(rightFoot.y);
                writer.Write(rightFoot.z);

                writer.Write(leftArm);
                writer.Write(rightArm);

                return stream.ToArray();
            }
        }

        public void UpdatePlayer()
        {
            position = player.transform.position;
            rotation = shadow.transform.rotation;

            leftHand = shadow.handIK_L.solver.arm.target.position - shadow.transform.position;
            rightHand = shadow.handIK_R.solver.arm.target.position - shadow.transform.position;

            leftKnee = shadow.realleftKnee.transform.position - shadow.transform.position;
            rightKnee = shadow.realrightKnee.transform.position - shadow.transform.position;

            leftFoot = shadow.footIK_L.solver.target.position - shadow.transform.position;
            rightFoot = shadow.footIK_R.solver.target.position - shadow.transform.position;

            leftArm = shadow.handIK_L.solver.arm.armLengthMlp;
            rightArm = shadow.handIK_R.solver.arm.armLengthMlp;

        }
    }
}