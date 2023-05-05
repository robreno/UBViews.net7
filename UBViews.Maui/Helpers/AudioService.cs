using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBViews.Services;

using UBViews.Models.Audio;

namespace UBViews.Helpers
{
    public partial class AudioService
    {
        IAudioService audioService;
        public AudioService(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        public async Task<AudioMarker> GetAt(int index)
        {
            var marker = await audioService.GetAt(index);
            return marker;
        }

        public async Task Clear()
        {
            await audioService.Clear();
        }

        public async Task Insert(AudioMarker mediaMarker)
        {
            await audioService.Insert(mediaMarker);
        }
    }
}
