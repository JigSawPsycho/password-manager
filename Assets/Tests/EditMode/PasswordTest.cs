using System;
using NUnit.Framework;

public class PasswordTest
{
    private Password password;
    private string passwordTestKey = "101192103106127041122048023150134013004019046019";
    private string testEncryptionKey = "HRyUCjETzckQ6A16TTO4qn2zIzv1FEwBvfWgUa+YAWg=";
    private string testEncryptionVector = "HNQyIt8rGp3VavKd1G62/w==";
    private string expectedPassword = "/_*oapVk_ku7";

    [SetUp]
    public void SetUp()
    {
        password = new()
        {
            key = passwordTestKey,
            name = "testPassword"
        };
    }

    [Test]
    public void GetPassword_ReturnsCorrectPassword()
    {
        string actualPassword = password.GetPassword(Convert.FromBase64String(testEncryptionKey), Convert.FromBase64String(testEncryptionVector));
        Assert.True(actualPassword == expectedPassword);
    }
}
