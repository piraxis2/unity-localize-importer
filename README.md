# Simple Localize Package

유니티 공식 Localization 패키지를 더 쉽고 직관적으로 사용할 수 있게 도와주는 패키지입니다. 엑셀 데이터를 테이블로 가져오는 Importer와, UI에 즉시 번역을 적용하는 Component를 포함하고 있습니다.

## 주요 기능
*   **Excel Importer**: ScriptableObject 형태의 엑셀 데이터를 String Table로 한 번에 동기화합니다.
*   **SimpleLocalizedText**: TextMeshProUGUI와 연동하여 테이블/키를 선택하고 에디터에서 즉시 미리보기를 제공합니다.
*   **Editor Preview**: 런타임뿐만 아니라 에디터 모드에서도 선택한 키의 번역 내용을 즉시 확인할 수 있습니다.

---

## 사용 방법

### 1. 사전 준비
*   **Localization 설정**: `Project Settings > Localization`에서 사용할 언어(Locale)를 추가합니다 (예: ko, en).
*   **테이블 생성**: `Window > Asset Management > Localization Tables`에서 새로운 `String Table Collection`을 생성합니다.

### 2. UI에 번역 적용 (SimpleLocalizedText)
1.  `TextMeshPro - Text (UI)`가 있는 오브젝트에 `SimpleLocalizedText` 컴포넌트를 추가합니다.
2.  **Table Collection**: 인스펙터 드롭다운에서 미리 생성한 번역 테이블을 선택합니다.
3.  **Key**: `Select Key...` 버튼을 눌러 번역할 키를 선택합니다. (검색 기능을 제공합니다)
4.  **확인**: 키를 선택하면 에디터 상에서 텍스트가 즉시 번역된 내용으로 바뀝니다.
    *   *주의: 선택된 언어(Selected Locale)가 없다면 프로젝트의 첫 번째 언어로 미리보기를 보여줍니다.*

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

## 스크립트 활용
코드에서 동적으로 키를 바꾸거나 텍스트를 업데이트할 수 있습니다.

```csharp
using Simple.Localize;

public class MyUI : MonoBehaviour
{
    public SimpleLocalizedText localizedText;

    public void ChangeToItemName(string itemKey)
    {
        // 테이블 이름과 키 이름을 전달하여 즉시 변경
        localizedText.SetKey("MyItemTable", itemKey);
        
        // 수동 갱신이 필요한 경우
        localizedText.UpdateText();
    }
}
```

## 설치 요구 사항
*   Unity 2021.3 이상
*   Localization (com.unity.localization)
*   TextMeshPro (com.unity.textmeshpro)
