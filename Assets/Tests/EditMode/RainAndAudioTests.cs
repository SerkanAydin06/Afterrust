using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class RainAndAudioTests
    {
        [Test]
        public void Rain_StopsBeforeTheFirstRoofAndKeepsOutdoorDrops()
        {
            float justAboveRoof = DayNightWeather.LifetimeToSurface(10f, 22f, 9.81f * 0.3f, 1.1f);
            float nearRoof = DayNightWeather.LifetimeToSurface(2f, 22f, 9.81f * 0.3f, 1.1f);
            float ground = DayNightWeather.LifetimeToSurface(25f, 22f, 9.81f * 0.3f, 1.1f);

            Assert.Greater(justAboveRoof, 0.35f);
            Assert.Less(justAboveRoof, 0.5f);
            Assert.Less(nearRoof, justAboveRoof);
            Assert.AreEqual(1.1f, ground, 0.0001f);
            Assert.AreEqual(0f, DayNightWeather.LifetimeToSurface(0.1f, 22f, 3f, 1.1f));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Ambience_GeneratesNewNoiseForEveryBufferWithoutSilenceAtTheJoin(bool rain)
        {
            const int samplesPerSecond = 22050;
            AfterrustAudio.ContinuousAmbientNoise source =
                new AfterrustAudio.ContinuousAmbientNoise(rain, 123, samplesPerSecond);
            float[] a = new float[samplesPerSecond * 2];
            float[] b = new float[samplesPerSecond * 2];
            source.Fill(a);
            source.Fill(b);

            // Silence at the end of every old two-second clip made the seam obvious.
            Assert.Greater(Rms(a, a.Length - 1024, 1024), 0.005f);
            Assert.Greater(Rms(b, 0, 1024), 0.005f);
            Assert.Greater(Rms(b, b.Length - 1024, 1024), 0.005f);
            Assert.AreNotEqual(a[20000], b[20000], "New buffers must not repeat the old sample");
        }

        private static float Rms(float[] samples, int start, int length)
        {
            double total = 0d;
            for (int i = start; i < start + length; i++)
                total += samples[i] * samples[i];
            return Mathf.Sqrt((float)(total / length));
        }
    }
}
