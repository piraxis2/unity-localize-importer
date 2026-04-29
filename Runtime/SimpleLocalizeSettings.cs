using UnityEngine;

namespace Simple.Localize
{
    /// <summary>
    /// 로컬라이즈 패키지의 전역 설정을 저장하는 에셋
    /// </summary>
    public class SimpleLocalizeSettings : ScriptableObject
    {
        [Header("Default Table Settings")]
        [Tooltip("기본적으로 사용할 텍스트 테이블 이름")]
        public string defaultTextTable = "Localize";

        [Tooltip("기본적으로 사용할 폰트 테이블 이름")]
        public string defaultFontTable = "FontTable";

        private static SimpleLocalizeSettings _instance;

        /// <summary>
        /// 설정 에셋 인스턴스를 가져옵니다. (Resources 폴더에서 로드)
        /// </summary>
        public static SimpleLocalizeSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SimpleLocalizeSettings>("SimpleLocalizeSettings");
                    
                    // 만약 에셋이 없다면 임시 인스턴스라도 생성하여 에러 방지
                    if (_instance == null)
                    {
                        _instance = CreateInstance<SimpleLocalizeSettings>();
                    }
                }
                return _instance;
            }
        }
    }
}
