using System.Collections.Generic;
using Amver.Domain.Localization;

namespace Amver.Domain.StaticMembers
{
    public static class Genders
    {
        public static readonly Dictionary<int, string> GenderList = new Dictionary<int, string>
            {
                {1, AppResources.GenderMale},
                {2, AppResources.GenderFemale},
                {3, AppResources.GenderNoMatter},
                {4, "Unknown"}
            };
    }
}