using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class MultiplayerInit : MonoBehaviour
{
    async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            InitializationOptions options = new InitializationOptions();
            string randomProfileName = "Player_" + UnityEngine.Random.Range(0, 99999);
            options.SetProfile(randomProfileName);

            // 유니티 클라우드 서비스 초기화
            await UnityServices.InitializeAsync(options);

            // 릴레이 서버 사용을 위한 익명 로그인
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"유니티 클라우드 로그인 성공! Player ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"클라우드 초기화 실패: {e.Message}");
        }
    }
}