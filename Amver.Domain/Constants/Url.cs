namespace Amver.Domain.Constants
{
    public static class Url
    {
        private const string BaseUrl = "https://amver.net/";
        
        public static string TripList = $"{BaseUrl}trip/list";

        public static string CityList = $"{BaseUrl}city/list";

        public static string CountryList = $"{BaseUrl}country/list";

        public static string Trip = $"{BaseUrl}trip/get?tripId=";

        public static string MyTrip = $"{BaseUrl}trip/getmytrip?tripId=";

        public static string FavouriteTripList = $"{BaseUrl}favouriteTrip/list";

        public static string AddTrip = $"{BaseUrl}trip/create";

        public static string IsFavouriteTrip = $"{BaseUrl}favouriteTrip/get";

        public static string Like = $"{BaseUrl}favouriteTrip/like";

        public static string UpdateTrip = $"{BaseUrl}trip/update";

        public static string GetMyTripList = $"{BaseUrl}trip/getMyTripList";

        public static string GetTokenByClassicLogin = $"{BaseUrl}auth/ClassicTokenLogin";

        public static string GetTokenByClassicTokenRegistration = $"{BaseUrl}auth/ClassicTokenRegistration";

        public static string GetMyProfile = $"{BaseUrl}profile/get";
        
        public static string GetMyProfileForUpdate = $"{BaseUrl}profile/getForUpdate";

        public static string GetFinishedRegistrationResult = $"{BaseUrl}auth/finishRegistration";

        public static string RemoveTrip = $"{BaseUrl}trip/remove?tripId=";

        public static string GetMyMainProfileInfo = $"{BaseUrl}user/getMyMainProfileInfo";

        public static string UploadUserPhoto = $"{BaseUrl}user/uploadUserPhoto";

        public static string GetUserProfile = $"{BaseUrl}profile/getById?userId=";

        public static string GetUserIcon = $"{BaseUrl}user/getUserIcon";

        public static string RemoveProfileImage = $"{BaseUrl}profile/removeImage";

        public static string GetMessageList = $"{BaseUrl}message/getList";

        public static string GetConversationList = $"{BaseUrl}conversation/getList";

        public static string GetProfileByFbMarker =
            "https://graph.facebook.com/v4.0/me?fields=id%2Cemail%2Cbirthday%2Cgender%2Cfirst_name%2Cpicture%2Clast_name&access_token=";

        public static string GetTokenByFacebookLogin = $"{BaseUrl}auth/FacebookTokenLogin?marker=";

        public static string CompleteTrip = $"{BaseUrl}trip/complete?tripId=";

        public static string Policy = "https://app.termly.io/document/privacy-notice/d303ea38-d9db-4135-995e-5cfc86cdf78e";

        public static string UpdateProfile = $"{BaseUrl}profile/update";
        
        public static string RemoveProfile = $"{BaseUrl}profile/remove";
        
        public static string GetLogin = $"{BaseUrl}auth/getLogin";
        
        public static string MyContactMail = "http://sipakov1987@gmail.com";

        public static string RemoveConversation = $"{BaseUrl}conversation/remove?conversationId=";
        
        public static string BanUser = $"{BaseUrl}user/ban";

        public static string SignOut = $"{BaseUrl}auth/SignOut";
        
        public static string BlockUser = $"{BaseUrl}user/block?userId=";
        
        public static string AuthorizedTripList = $"{BaseUrl}trip/authorizedList";
        
        public static string CheckMyBlocked = $"{BaseUrl}user/checkMyBlocked?userId=";
        
        public static string GetAuthUserProfile = $"{BaseUrl}profile/getAuthById?userId=";
        
        public static string GetAuthTrip = $"{BaseUrl}trip/getAuthTrip?tripId=";
        
        public static string CheckForMyAndMeBlocked = $"{BaseUrl}user/checkForMyAndMeBlocked?userId=";
        
        public static string UnblockUser = $"{BaseUrl}user/unblock?userId=";
        
        public static string AddUserFcmToken = $"{BaseUrl}auth/addUserFcmToken";

        public static string MessageIsRead = $"{BaseUrl}message/isRead";
    }
}