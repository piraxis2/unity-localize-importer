# Simple Localize Package

유니티 공식 Localization 패키지를 더 쉽고 직관적으로 사용할 수 있게 도와주는 패키지입니다. 엑셀 데이터를 테이블로 가져오는 Importer와, UI에 즉시 번역 및 폰트를 적용하는 Component를 포함하고 있습니다.

## 주요 기능
*   **Excel Importer**: ScriptableObject 형태의 엑셀 데이터를 String Table로 한 번에 동기화합니다.
*   **SimpleLocalizedText**: TextMeshProUGUI와 연동하여 텍스트와 폰트를 언어별로 자동 변경합니다.
*   **Smart String 지원**: 인스펙터나 코드에서 매개변수(Arguments)를 전달하여 동적 텍스트를 구성할 수 있습니다.
*   **Font Localization**: 언어별로 다른 폰트 에셋(TMP_FontAsset)을 할당하여 언어 변경 시 텍스트와 폰트가 함께 바뀌도록 지원합니다.
*   **Editor Preview**: 에디터 모드에서도 선택한 키와 폰트의 적용 결과를 즉시 확인할 수 있습니다.

---

## 사용 방법

### 1. UI에 번역 및 폰트 적용 (SimpleLocalizedText)
1.  `TextMeshPro - Text (UI)` 오브젝트에 `SimpleLocalizedText` 컴포넌트를 추가합니다.
2.  **Text Localization**:
    *   **Table**: 사용할 String Table을 선택합니다.
    *   **Key**: `Select Key...` 버튼을 눌러 번역 키를 선택합니다.
3.  **Smart String Arguments (선택)**:
    *   텍스트 내용에 `{0}`, `{1}` 등이 포함된 경우, 리스트에 값을 추가하여 채울 수 있습니다.
4.  **Font Localization (선택)**:
    *   언어별로 폰트를 다르게 하려면 `Asset Table`을 생성하고 폰트 에셋들을 등록해야 합니다.
    *   **Font Table**: 폰트 에셋이 담긴 Asset Table을 선택합니다.
    *   **Font Key**: 해당 폰트의 키를 선택합니다.

### 2. 코드에서 활용하기

```csharp
using Simple.Localize;

public class MyUI : MonoBehaviour
{
    public SimpleLocalizedText localizedText;

    public void UpdatePlayerInfo(string playerName, int level)
    {
        // 1. 매개변수 전달 (예: "안녕, {0}! 레벨: {1}")
        localizedText.SetArgs(playerName, level.ToString());
        
        // 2. 동적으로 키 변경
        localizedText.SetKey("MyTable", "NewKey");
        
        // 3. 동적으로 폰트 변경
        localizedText.SetFontKey("FontTable", "MainFont");
    }
}
```

---

### 3. 데이터 가져오기 (Excel Importer)
1.  상단 메뉴에서 **Tools > Simple Localize > Importer**를 클릭합니다.
2.  **Source Asset**: 엑셀 데이터가 담긴 ScriptableObject를 할당합니다.
3.  **Target String Table**: 동기화할 String Table Collection을 할당합니다.
4.  **Field Settings**: 
    *   `List Field Name`: 리스트 변수명 (기본: Data)
    *   `Key Field Name`: 키 변수명 (기본: Key)
5.  **Locale Mapping**: 엑셀 컬럼명과 유니티 로케일 코드를 매핑합니다.
6.  **[Import / Update Table]** 버튼을 눌러 실행합니다.

---

## 설치 요구 사항
*   Unity 2021.3 이상
*   Localization (com.unity.localization)
*   TextMeshPro (com.unity.textmeshpro)

---

## 트러블슈팅 (Troubleshooting)
*   **에디터 프리뷰가 보이지 않는 경우**: 
    *   프로젝트를 처음 열거나 스크립트 컴파일 직후에는 Localization 설정이 완전히 로드되지 않아 미리보기가 작동하지 않을 수 있습니다.
    *   이 경우 **Play Mode(재생)**를 한 번 실행하여 UI를 로드하면, 시스템이 초기화되어 이후부터는 에디터 상에서도 정상적으로 미리보기가 작동합니다.