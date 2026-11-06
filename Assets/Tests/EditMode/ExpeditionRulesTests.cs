using NUnit.Framework;

namespace Afterrust.Tests
{
    public sealed class ExpeditionRulesTests
    {
        [Test]
        public void ChemicalExposure_IsZeroOutsideRiskArea_AndMaxAtCentre()
        {
            Assert.AreEqual(0f, ToxicExposure.ExposureIntensity(3.45f, 3.45f), 0.0001f);
            Assert.AreEqual(0f, ToxicExposure.ExposureIntensity(5f, 3.45f), 0.0001f);
            Assert.AreEqual(0f, ToxicExposure.ExposureIntensity(0f, 0f), 0.0001f);
            Assert.AreEqual(1f, ToxicExposure.ExposureIntensity(0f, 3.45f), 0.0001f);
            float halfway = ToxicExposure.ExposureIntensity(2.5f, 3.45f);
            Assert.Greater(halfway, 0f);
            Assert.Less(halfway, 1f);
        }

        [Test]
        public void Expedition_RequiresSearchAndReturnBeforeCompletion()
        {
            Assert.AreEqual(ExpeditionPhase.FindExit,
                FirstExpeditionGuide.DecidePhase(false, false, true, false));
            Assert.AreEqual(ExpeditionPhase.SearchCheckpoint,
                FirstExpeditionGuide.DecidePhase(true, false, false, false));
            Assert.AreEqual(ExpeditionPhase.ReturnToShelter,
                FirstExpeditionGuide.DecidePhase(true, true, false, false));
            Assert.AreEqual(ExpeditionPhase.Complete,
                FirstExpeditionGuide.DecidePhase(true, true, true, false));
            Assert.AreEqual(ExpeditionPhase.Complete,
                FirstExpeditionGuide.DecidePhase(false, true, false, true));
        }
    }
}
