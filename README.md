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
2.  **Localization Key**: 엑셀에 정의된 키(예: `btn_start`)를 입력합니다.
3.  에디터 상에서도 즉시 텍스트가 바뀌어 미리보기가 가능합니다.

### 4. 코드에서 언어 변경
게임 실행 중 언어를 변경하려면 `SettingsManager` 혹은 아래 코드를 사용합니다.
```csharp
// 유니티 로컬라이제이션 설정을 직접 변경
UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = 
    UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale("en");
```