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
`SimpleLocalizedText` 컴포넌트의 인스펙터 항목들은 다음과 같은 역할을 합니다.

*   **Table Collection (드롭다운)**
    *   사용할 번역 테이블(예: `Localize`, `Quest`, `Item`)을 선택합니다.
*   **Localized String (드롭다운/검색)**
    *   테이블 내의 **Key(고유 ID)**를 선택합니다.
    *   타이핑하여 키를 검색할 수 있으며, 선택 시 해당 키에 연결된 텍스트가 표시됩니다.
*   **Add Table Entry (+ 버튼)**
    *   새로운 Key와 번역 텍스트를 현재 테이블에 즉시 추가합니다. 엑셀을 열지 않고도 간단한 수정이 가능합니다.
*   **Edit Table Entry (연필 버튼)**
    *   선택된 Key의 번역 내용(한국어, 영어 등)을 수정할 수 있는 **Localization Tables** 창을 엽니다.
    *   *이 창에서 `Add Locale` 버튼을 눌러 새로운 언어(일본어 등)를 추가할 수 있습니다.*
*   **Local Variables (Smart String)**
    *   텍스트 안에 `{0}`, `{name}` 같은 변수가 포함된 경우(Smart String), 이곳에 실제 값을 연결할 수 있습니다.
    *   예: "Hello, {0}!" → Local Variables에 "User" 입력 → 결과: "Hello, User!"
*   **참고: 기타 옵션**
    *   `Enable Fallback` (Settings): 번역이 없을 때 기본 언어(영어)로 대체할지 여부.
    *   `Wait for completion`: (공식 컴포넌트 옵션) 로딩이 완료될 때까지 빈 화면을 유지할지 여부. (`SimpleLocalizedText`는 내부적으로 자동 처리됨)

### 5. 코드에서 언어 변경
게임 실행 중 언어를 변경하려면 `SettingsManager` 혹은 아래 코드를 사용합니다.
```csharp
// 유니티 로컬라이제이션 설정을 직접 변경
UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = 
    UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale("en");
```