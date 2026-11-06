using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class SafeZoneGateTests
    {
        [Test]
        public void BothPanels_AreControlledByASingleInteractable_AndSwingInwards()
        {
            GameObject root = new GameObject("South Safe Zone Gate");
            try
            {
                SafeZoneGate gate = root.AddComponent<SafeZoneGate>();
                GameObject left = new GameObject("Left Hinge");
                GameObject right = new GameObject("Right Hinge");
                left.transform.SetParent(root.transform);
                right.transform.SetParent(root.transform);
                gate.leftHinge = left.transform;
                gate.rightHinge = right.transform;
                gate.openAngle = 100f;

                GameObject panel = new GameObject("Gate Panel");
                panel.transform.SetParent(left.transform);
                BoxCollider collider = panel.AddComponent<BoxCollider>();
                Assert.AreSame(gate, collider.GetComponentInParent<IInteractable>());

                Assert.IsFalse(gate.isOpen, "Perimeter gate must start closed");
                Assert.AreEqual("E - Güvenli bölge kapısını aç", gate.GetPrompt(null));
                gate.Interact(null);
                Assert.IsTrue(gate.isOpen);
                gate.SetOpen(true, true);
                Assert.AreEqual(-100f, gate.LeftAngle, 0.01f);
                Assert.AreEqual(100f, gate.RightAngle, 0.01f);
                Assert.AreEqual(260f, left.transform.localEulerAngles.y, 0.01f);
                Assert.AreEqual(100f, right.transform.localEulerAngles.y, 0.01f);
                gate.SetOpen(false, true);
                Assert.AreEqual(0f, gate.LeftAngle, 0.01f);
                Assert.AreEqual(0f, gate.RightAngle, 0.01f);
                Assert.AreEqual("E - Güvenli bölge kapısını aç", gate.GetPrompt(null));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Gate_StateIsIncludedInExistingDoorSaveFormat()
        {
            GameObject yard = new GameObject("Scrapyard Environment");
            GameObject player = new GameObject("Save Test Player");
            try
            {
                GameObject gateObject = new GameObject("South Safe Zone Gate");
                gateObject.transform.SetParent(yard.transform);
                SafeZoneGate gate = gateObject.AddComponent<SafeZoneGate>();
                GameObject left = new GameObject("Left");
                GameObject right = new GameObject("Right");
                left.transform.SetParent(gateObject.transform);
                right.transform.SetParent(gateObject.transform);
                gate.leftHinge = left.transform;
                gate.rightHinge = right.transform;
                gate.SetOpen(true, true);

                SaveSystem saver = player.AddComponent<SaveSystem>();
                SaveSystem.SaveData data = saver.Capture();
                string path = SaveSystem.PathOf(gateObject.transform);
                Assert.IsTrue(data.doors.Exists(d => d.id == path && d.open),
                    "Gate state must survive saves just like the shelter door");
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(yard);
            }
        }
    }
}
