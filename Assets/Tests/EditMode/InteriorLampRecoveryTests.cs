using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    /// <summary>
    /// The lighting migration lives in the editor-only assembly. Use reflection
    /// to verify its selectors/reconciliation without mutating project materials
    /// or rebuilding the live scene in EditMode.
    /// </summary>
    public sealed class InteriorLampRecoveryTests
    {
        private static MethodInfo EditorMethod(string methodName)
        {
            Type type = null;
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType("Afterrust.EditorTools.ShelterInteriorLightingPass");
                if (type != null) break;
            }
            Assert.IsNotNull(type, "Editor lighting pass must be compiled.");
            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, methodName);
            return method;
        }

        [Test]
        public void MissingLightChild_DoesNotHideAnExistingWorkbenchFixture()
        {
            GameObject root = new GameObject("Scrapyard Lamps");
            try
            {
                GameObject bench = new GameObject("Wall Lamp");
                bench.transform.SetParent(root.transform, false);
                bench.transform.position = new Vector3(-1.6f, 2.45f, 5.87f);

                GameObject distant = new GameObject("Wall Lamp");
                distant.transform.SetParent(root.transform, false);
                distant.transform.position = new Vector3(-3.2f, 2.5f, -0.13f);

                Transform found = (Transform)EditorMethod("FindWorkbenchLamp").Invoke(
                    null, new object[] { root.transform });
                Assert.AreSame(bench.transform, found);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void MissingPendant_IsDetectedWithoutRetuningUnrelatedOutdoorLights()
        {
            GameObject root = new GameObject("Scrapyard Lamps");
            try
            {
                GameObject gate = new GameObject("Wall Lamp");
                gate.transform.SetParent(root.transform, false);
                gate.transform.position = new Vector3(-4f, 2.8f, -19.9f);

                Assert.IsNull(EditorMethod("FindPendantLamp").Invoke(
                    null, new object[] { root.transform }));
                GameObject pendant = new GameObject("Pendant Lamp");
                pendant.transform.SetParent(root.transform, false);
                pendant.transform.position = new Vector3(0.3f, 2.45f, 3f);
                Assert.AreSame(pendant.transform, EditorMethod("FindPendantLamp").Invoke(
                    null, new object[] { root.transform }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void WeatherRewiring_IsIdempotentAndPreservesExternalTraderLight()
        {
            GameObject root = new GameObject("Scrapyard Lamps");
            GameObject worldObject = new GameObject("World Environment");
            GameObject external = new GameObject("Trader Night Lamp");
            try
            {
                DayNightWeather weather = worldObject.AddComponent<DayNightWeather>();
                Light traderLight = external.AddComponent<Light>();
                Renderer traderBulb = GameObject.CreatePrimitive(PrimitiveType.Sphere)
                    .GetComponent<Renderer>();
                traderBulb.name = "Bulb";
                traderBulb.transform.SetParent(external.transform, false);

                GameObject fixture = new GameObject("Wall Lamp");
                fixture.transform.SetParent(root.transform, false);
                GameObject lightObject = new GameObject("Light");
                lightObject.transform.SetParent(fixture.transform, false);
                Light lamp = lightObject.AddComponent<Light>();
                GameObject lampBulbObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lampBulbObject.name = "Bulb";
                lampBulbObject.transform.SetParent(fixture.transform, false);
                Renderer lampBulb = lampBulbObject.GetComponent<Renderer>();

                weather.lamps = new[] { traderLight };
                weather.lampBulbs = new[] { traderBulb };
                MethodInfo method = EditorMethod("SyncWeatherReferences");
                method.Invoke(null, new object[] { root.transform });
                method.Invoke(null, new object[] { root.transform });

                Assert.AreEqual(2, weather.lamps.Length, "Repair must not duplicate lights.");
                CollectionAssert.Contains(weather.lamps, lamp);
                CollectionAssert.Contains(weather.lamps, traderLight);
                Assert.AreEqual(2, weather.lampBulbs.Length);
                CollectionAssert.Contains(weather.lampBulbs, lampBulb);
                CollectionAssert.Contains(weather.lampBulbs, traderBulb);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(worldObject);
                UnityEngine.Object.DestroyImmediate(external);
            }
        }
    }
}
