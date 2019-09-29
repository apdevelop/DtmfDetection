namespace DtmfDetection.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using global::NAudio;
    using global::NAudio.Wave;
    using global::NAudio.Wave.SampleProviders;

    using DtmfDetection.NAudio;

    /// <summary>
    /// DTMF tones detection tests using generated tones (NAudio SignalGenerator)
    /// </summary>
    [TestClass]
    public class GeneratedTonesTests
    {
        private static string TempFilePath
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), "dtmftest.wav"); // TODO: use in-memory temp wav storage
            }
        }

        private static DtmfTone[] AllTones
        {
            get
            {
                return new[]
                {
                    DtmfTone.One,
                    DtmfTone.Two,
                    DtmfTone.Three,
                    DtmfTone.Four,
                    DtmfTone.Five,
                    DtmfTone.Six,
                    DtmfTone.Seven,
                    DtmfTone.Eight,
                    DtmfTone.Nine,
                    DtmfTone.Zero,
                    DtmfTone.Hash,
                    DtmfTone.Star,
                    DtmfTone.A,
                    DtmfTone.B,
                    DtmfTone.C,
                    DtmfTone.D,
                };
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(TempFilePath);
        }

        [TestMethod]
        public void DetectsSingleCleanTone()
        {
            foreach (var tone in AllTones)
            {
                GenerateToneWaveFile(TempFilePath, DtmfTone.Hash, 8000, 250.0);

                using (var audioFile = new AudioFileReader(TempFilePath))
                {
                    var tones = audioFile.DtmfTones().ToArray();
                    Assert.AreEqual(1, tones.Length);
                    Assert.AreEqual(DtmfTone.Hash, tones[0].DtmfTone);
                    // TODO: why tone Position starts at 26 ms ?
                }
            }
        }

        [TestMethod]
        public void DetectsSingleToneWithWhiteNoise()
        {
            foreach (var tone in AllTones)
            {
                GenerateToneWaveFile(TempFilePath, tone, 8000, 250.0, new SignalGenerator(8000, 1)
                {
                    Gain = 0.6,
                    Type = SignalGeneratorType.White,
                });

                using (var audioFile = new AudioFileReader(TempFilePath))
                {
                    var tones = audioFile.DtmfTones().ToArray();
                    Assert.AreEqual(1, tones.Length);
                    Assert.AreEqual(tone, tones[0].DtmfTone);
                }
            }
        }

        [TestMethod]
        public void DetectsSingleToneWithPinkNoise()
        {
            foreach (var tone in AllTones)
            {
                GenerateToneWaveFile(TempFilePath, tone, 8000, 250.0, new SignalGenerator(8000, 1)
                {
                    Gain = 0.6,
                    Type = SignalGeneratorType.Pink,
                });

                using (var audioFile = new AudioFileReader(TempFilePath))
                {
                    var tones = audioFile.DtmfTones().ToArray();
                    Assert.AreEqual(1, tones.Length);
                    Assert.AreEqual(tone, tones[0].DtmfTone);
                }
            }
        }

        [TestMethod]
        public void DetectsSingleToneWith50HzNoise()
        {
            foreach (var tone in AllTones)
            {
                GenerateToneWaveFile(TempFilePath, tone, 8000, 250.0, new SignalGenerator(8000, 1)
                {
                    Gain = 0.6,
                    Frequency = 50.0,
                    Type = SignalGeneratorType.Sin,
                });

                using (var audioFile = new AudioFileReader(TempFilePath))
                {
                    var tones = audioFile.DtmfTones().ToArray();
                    Assert.AreEqual(1, tones.Length);
                    Assert.AreEqual(tone, tones[0].DtmfTone);
                }
            }
        }

        private void GenerateToneWaveFile(string path, DtmfTone tone, int sampleRate, double duration, ISampleProvider noise = null)
        {
            var tone1 = new SignalGenerator(sampleRate, 1)
            {
                Gain = 0.5,
                Frequency = tone.LowTone,
                Type = SignalGeneratorType.Sin
            }
            .Take(TimeSpan.FromMilliseconds(duration));

            var tone2 = new SignalGenerator(sampleRate, 1)
            {
                Gain = 0.5,
                Frequency = tone.HighTone,
                Type = SignalGeneratorType.Sin
            }
            .Take(TimeSpan.FromMilliseconds(duration));

            var list = new List<ISampleProvider>(new[] { tone1, tone2 });

            if (noise != null)
            {
                list.Add(noise.Take(TimeSpan.FromMilliseconds(duration)));
            }

            var mix = new MixingSampleProvider(list);

            WaveFileWriter.CreateWaveFile16(path, mix);
        }
    }
}