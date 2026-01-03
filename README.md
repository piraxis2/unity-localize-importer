# 사용 방법

### 1. 사전 준비
*   **Localization 설정**: `Project Settings > Localization`에서 사용할 언어(Locale)를 추가합니다 (예: ko, en).
*   **테이블 생성**: `Window > Asset Management > Localization Tables`에서 새로운 `String Table Collection`을 생성합니다 (이름을 `Localize`로 추천).

### 2. 데이터 가져오기 (Import)
1.  상단 메뉴에서 **Tools > Simple Localize > Importer**를 클릭합니다.
2.  **Source Asset (Excel)**: 엑셀 데이터가 담긴 ScriptableObject를 할당합니다.
3.  **Target String Table**: 준비한 공식 String Table Collection을 할당합니다.
4.  **Field Settings**:
    *   `List Field Name`: 데이터 리스트의 변수 이름 (기본값: `Data`)
    *   `Key Field Name`: 키 컬럼의 변수 이름 (기본값: `Key`)
5.  **Locale Mapping**: 엑셀의 컬럼 이름과 유니티의 언어 코드를 연결합니다. (예: `KR` -> `ko`, `EN` -> `en`)
6.  **[Import / Update Table]** 버튼을 눌러 동기화를 완료합니다.

### 3. UI 자동 적용 (SimpleLocalizedText)
UI 텍스트가 자동으로 번역되게 하려면:
1.  `TextMeshPro - Text (UI)`가 있는 오브젝트에 `SimpleLocalizedText` 컴포넌트를 추가합니다.
2.  **Localized String**: 인스펙터에서 테이블을 선택하고 키를 검색하여 할당합니다.
3.  에디터 상에서도 즉시 텍스트가 바뀌어 미리보기가 가능합니다.

### 4. 인스펙터 UI 상세 설명
`SimpleLocalizedText` 컴포넌트의 `Localized String` 항목은 다음과 같은 편리한 기능을 제공합니다.

*   **Table Selector (드롭다운)**: 사용할 스트링 테이블(예: Localize)을 선택합니다.
*   **Key Selector (검색창)**: 선택한 테이블 내의 키 값을 검색하고 선택합니다. 엑셀에서 추가한 키들이 이곳에 나열됩니다.
*   **아이콘 버튼들**:
    *   **돋보기/목록**: 전체 키 목록을 팝업창으로 띄워 시각적으로 검색합니다.
    *   **연필 (Edit)**: 선택된 키의 번역 내용을 즉석에서 수정합니다.
    *   **더하기 (+)**: 새로운 키와 번역 내용을 테이블에 즉시 추가합니다.

### 5. 코드에서 언어 변경
게임 실행 중 언어를 변경하려면 `SettingsManager` 혹은 아래 코드를 사용합니다.
```csharp
// 유니티 로컬라이제이션 설정을 직접 변경
UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = 
    UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale("en");
```
