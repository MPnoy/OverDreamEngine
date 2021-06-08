using System.Collections.Generic;
using System.Diagnostics;
using ODEngine.Core;
using ODEngine.Game;
using ODEngine.Game.Images;
using ODEngine.Helpers;

public class CommandForm
{
    public bool isImage = true;
    public bool isDestroy;

    //Image
    public ScenarioStep.ImageType imageType;
    public string objectName;
    public Composition composition;
    public ColorMatrix colorMatrix = ColorMatrix.Identity;
    public Material transitionMaterial;
    public float transitionTime = 2f;
    public int loopIndex = -1;
    public ScenarioStep.TextAnimationInfo textAnimation = null;
    public int zLevel;

    //Sound
    public ScenarioStep.SoundType soundType;
    public float volume = 0.8f;

    public CommandForm() { }

    public CommandForm(bool isImage)
    {
        this.isImage = isImage;
    }

    public ScenarioStep.Data ConvertToCData()
    {
        if (isImage)
        {
            if (textAnimation == null)
            {
                textAnimation = new ScenarioStep.TextAnimationInfo("None", new List<ScenarioStep.TextAnimationInfo.Var>());
            }
            return isDestroy
                ? new ScenarioStep.DataRemoveImage(imageType, objectName, transitionTime, textAnimation)
                : (ScenarioStep.Data)new ScenarioStep.DataAddImage(imageType, objectName, new ImageRequestData((ImageComposition)composition, colorMatrix), transitionMaterial, transitionTime, textAnimation, zLevel);
        }
        else
        {
            if (isDestroy)
            {
                return new ScenarioStep.DataRemoveSound(soundType, objectName, transitionTime);
            }
            else
            {
                if (composition == null)
                {
                    Debug.Print("Композиции музычки нет: " + objectName);
                }

                return new ScenarioStep.DataAddSound(soundType, objectName, (AudioComposition)composition, transitionTime, volume, loopIndex);
            }
        }
    }
}
