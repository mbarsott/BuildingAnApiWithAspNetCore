using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class TalkProfile: Profile
    {
        public TalkProfile()
        {
            this.CreateMap<Talk, TalkModel>();
        } 
    }
}
