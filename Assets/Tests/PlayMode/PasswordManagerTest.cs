using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PasswordManagerTest : MonoBehaviour, IMonoBehaviourTest
{
    [SerializeField]
    private PasswordManager passwordManager;
    private bool isTestFinished;
    public bool IsTestFinished => isTestFinished;
    [SerializeField]
    private PasswordUI passwordPrefab;
    [SerializeField]
    private Transform passwordsContainer;
    [SerializeField]
    private PromptUI promptUI;
    [SerializeField]
    private AddPasswordUI addPasswordUI;
    public static string TestEncryptionProfilePath => Path.Combine(Application.persistentDataPath, "tests", "testprofile.pgenprof");
    public string TestPasswordPath => Path.Combine(passwordManager.PasswordsDir, "testpassword.pgenpw");
    public string TestDuplicatePasswordPath => Path.Combine(passwordManager.PasswordsDir, "DUPLICATEPWD.pgenpw");

    [OneTimeSetUp]
    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
        passwordPrefab = GameObject.Find("Canvas/UIContainer/MainViewContainer/Content/PasswordsList/Scroll View/Viewport/Content/Password").GetComponent<PasswordUI>();
        passwordsContainer = GameObject.Find("Canvas/UIContainer/MainViewContainer/Content/PasswordsList/Scroll View/Viewport/Content").transform;
        promptUI = GameObject.Find("Canvas/UIContainer/MainViewContainer/PromptWindow").GetComponent<PromptUI>();
        addPasswordUI = GameObject.Find("Canvas/UIContainer/MainViewContainer/NewPasswordWindow").GetComponent<AddPasswordUI>();
        using (StreamReader reader1 = new StreamReader((Stream)File.OpenRead(TestEncryptionProfilePath)))
        {
            using (JsonTextReader reader2 = new JsonTextReader((TextReader)reader1))
            {
                EncryptionProfile encryptionProfile = jsonSerializer.Deserialize<EncryptionProfile>((JsonReader)reader2);

                encryptionProfile.profileName = Path.GetFileNameWithoutExtension(TestEncryptionProfilePath);
                passwordManager = new PasswordManager(passwordPrefab, passwordsContainer, promptUI, encryptionProfile);
                passwordManager.PopulatePasswordList(passwordManager.GetPasswords());
            }
        }
    }

    [TearDown]
    public void Teardown()
    {
        if(File.Exists(TestPasswordPath)) File.Delete(TestPasswordPath);
        if(File.Exists(TestDuplicatePasswordPath)) File.Delete(TestDuplicatePasswordPath);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void CreatePassword_GivenValidPasswordInfo_ReturnsValidPassword()
    {
        AutoCreatePasswordViaUI(addPasswordUI, Path.GetFileNameWithoutExtension(TestPasswordPath), 15);

        Assert.IsTrue(File.Exists(TestPasswordPath), "Password file exists after creation");
    }

    [Test]
    public void CreatePassword_GivenDuplicatePasswordName_ShowsError()
    {
        AutoCreatePasswordViaUI(addPasswordUI, Path.GetFileNameWithoutExtension(TestDuplicatePasswordPath), 15);

        AutoCreatePasswordViaUI(addPasswordUI, Path.GetFileNameWithoutExtension(TestDuplicatePasswordPath), 15);

        Assert.IsTrue(addPasswordUI.errorText.gameObject.activeSelf && !string.IsNullOrEmpty(addPasswordUI.errorText.text), "Error message shows after duplicate password input");
    }

    private void AutoCreatePasswordViaUI(AddPasswordUI addPasswordUI, string passwordName, int charCount)
    {
        passwordManager.CreatePassword(addPasswordUI);
        
        Assert.IsTrue(addPasswordUI.uiContainer.activeSelf, "AddPasswordUI is active");

        addPasswordUI.passwordName.text = passwordName;
        addPasswordUI.passwordLengthSlider.value = charCount;
        addPasswordUI.submitButton.onClick.Invoke();
    }
}
