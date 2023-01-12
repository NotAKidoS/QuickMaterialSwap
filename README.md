# QuickMaterialSwap
Unity Editor script to make changing materials on an object &amp; its children much quicker.

I built this script to make swapping materials on ChilloutVR avatars much easier.

![image](https://user-images.githubusercontent.com/37721153/212060755-94aa7de4-0a32-4f46-a17e-4a4d390301d6.png)
![image](https://user-images.githubusercontent.com/37721153/212060765-7dafc3b7-1427-4fa1-8343-cfae1224fd70.png)


## How to Use
* Open Unity and navigate to the "NotAKid" menu in the top menu bar.
* Select "Quick Material Swap" to open the Quick Material Swap window.

* In the "Selected Object" field, select the GameObject whose materials you want to view and edit.
* The script will automatically display all the materials used by the selected GameObject and its children in a list.
* Changing a material in the list will automatically change the material for any GameObject that used it.
* Changes are properly recorded using Unity's Undo System and can be undone using CTRL+Z.

* Once you're done editing the materials, you can close the Material Editor window.
