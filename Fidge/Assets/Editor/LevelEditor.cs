using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using NUnit.Framework.Constraints;
using UnityEngine.Assertions.Must;

[CustomEditor(typeof(EditableLevel))]
public class LevelEditor : Editor
{
    private enum ShiftDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public static List<HypotheticalSolution> Solutions;

    private bool _showInfo = true;
    private bool _showSolution = true;
    private bool _showHelp = true;
    private bool _shortcutKeyReleased = true;

    private EditableLevel _editableLevel;
    public EditableLevel EditableLevel
    {
        get
        {
            if (_editableLevel == null)
            {
                _editableLevel = (EditableLevel)target;
            }
            
            return _editableLevel;
        }
    }

    [MenuItem("Assets/Create/EditableLevel")]
    static void CreateLevel()
    {
        var path = "";
        path += AssetDatabase.GetAssetPath(Selection.activeObject);
        path += path != "" ? "/" : "Assets/";

        var newLevel = CreateInstance<EditableLevel>();
        ProjectWindowUtil.CreateAsset(newLevel, path + "Newlevel.asset");
        AssetDatabase.SaveAssets();
    }
    
    public override void OnInspectorGUI()
    {
        const float levelWidth = EditableLevel.KWidth * EditableLevel.KNodeSize;
        const float levelHeight = EditableLevel.KHeight * EditableLevel.KNodeSize;

        //const float editingAreaOffset = 50f;
        const float topOffset = 135;
        const float leftOffset = 65f;

        //var containerRect = new Rect((Screen.width - levelWidth) / 2, (Screen.height - levelHeight) / 2 + editingAreaOffset, levelWidth, levelHeight);
        var containerRect = new Rect(leftOffset, topOffset, levelWidth, levelHeight);
        
        const float buttonHeight = 40f;
        const float buttonWidth = 50f;
        const float buttonOffset = 5f;

        //EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button("Instantiate")) { InstantiateLevel(); }
            //if (GUILayout.Button("Delete Instance")) { DeleteInstance(); }
            if (GUILayout.Button("Clear")) { ClearLevel(); }
        //EditorGUILayout.EndHorizontal();

        DrawContainerRect(containerRect);

        if (GUI.Button(new Rect(levelWidth / 2 + leftOffset - buttonWidth / 2f, topOffset - (buttonHeight + buttonOffset), buttonWidth, buttonHeight),
            EditableLevel.LevelPrefab.GetComponent<Level>().UpArrow.texture))
        {
            ShiftLevel(ShiftDirection.Up);
        }
        
        if (GUI.Button(new Rect(levelWidth / 2 + leftOffset - buttonWidth / 2f, topOffset + levelHeight + buttonOffset, buttonWidth, buttonHeight),
            EditableLevel.LevelPrefab.GetComponent<Level>().DownArrow.texture))
        {
            ShiftLevel(ShiftDirection.Down);
        }

        if (GUI.Button(new Rect(leftOffset - (buttonHeight + buttonOffset), levelHeight / 2f - buttonWidth / 2f + topOffset, buttonHeight, buttonWidth),
            EditableLevel.LevelPrefab.GetComponent<Level>().LeftArrow.texture))
        {
            ShiftLevel(ShiftDirection.Left);
        }

        if (GUI.Button(new Rect(leftOffset + levelWidth + buttonOffset, levelHeight / 2f - buttonWidth / 2f + topOffset, buttonHeight, buttonWidth),
            EditableLevel.LevelPrefab.GetComponent<Level>().RightArrow.texture))
        {
            ShiftLevel(ShiftDirection.Right);
        }

        GUILayout.Space(levelHeight + buttonHeight * 2 + buttonOffset * 2 + 30);

        _showInfo = EditorGUILayout.Foldout(_showInfo, "Information");

        if (_showInfo)
        {
            EditableLevel.Scripted = GUILayout.Toggle(EditableLevel.Scripted, "Scripted");

            if (EditableLevel.Scripted)
            {
                var resetTutorialsProperty = serializedObject.FindProperty("ResetTutorials");
                serializedObject.Update();
                EditorGUILayout.PropertyField(resetTutorialsProperty, true);
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditableLevel.ExpectedTime = EditorGUILayout.IntField("Expected Time", EditableLevel.ExpectedTime);
                EditableLevel.ExpectedMoves = EditorGUILayout.IntField("Expected Moves", EditableLevel.ExpectedMoves);
                if (GUILayout.Button(string.Format("Apply suggested values (Time: {0}, Moves: {1})", GetSuggestedTime(), GetSuggestedMoves()))) { ApplySuggestedExpectedValues(EditableLevel); }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Description");
            EditableLevel.Desription = EditorGUILayout.TextArea(EditableLevel.Desription, GUILayout.MinHeight(30));
            EditorGUILayout.EndHorizontal();

            EditableLevel.Author = EditorGUILayout.TextField("Author", EditableLevel.Author);

            EditorGUILayout.Space();
        }

        if (!EditableLevel.Scripted)
        {
            _showSolution = EditorGUILayout.Foldout(_showSolution, "Solution");

            if (_showSolution)
            {
                EditableLevel.AllowedEndNodeBypasses = EditorGUILayout.IntField("Allowed End Node Bypasses", EditableLevel.AllowedEndNodeBypasses);

                if (GUILayout.Button("Compute possible solution(s)")) { ComputeSolution(EditableLevel); }

                var solutionString = string.Empty;

                if (EditableLevel.NumberOfSolutions == -1)
                {
                    solutionString = "Solution not yet computed";
                }
                else
                {
                    solutionString = string.Format("{0}{1}",
                        EditableLevel.NumberOfSolutions > 0 ? "Minimum moves: " + EditableLevel.MinimumMoves : string.Empty,
                        EditableLevel.NumberOfSolutions > 0 && EditableLevel.MinimumMovesWithFlag > 0 && EditableLevel.MinimumMovesWithFlag < 200 ? ". With flag(s): " + EditableLevel.MinimumMovesWithFlag : ". Flag Unreachable");
                }

                EditorGUILayout.LabelField(solutionString);
            }
        }

        _showHelp = EditorGUILayout.Foldout(_showHelp, "Help");

        if (_showHelp)
        {
            var myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            myStyle.fontSize = 11;

            EditorGUILayout.TextArea(
                "<b>Left-Click (Empty cell)</b> : Create node or path\n" +
                "<b>Left-Click (Empty path)</b> : Change path orientation\n" +
                "<b>Left-Click (Obstacle or collectable)</b> : Cycle through colors or directions\n" +
                "<b>Right-Click</b> : Open context menu\n" +
                "<b>Backspace</b> : Clear cell\n" +
                "<b>C</b> : Change traversal state modifier to Covered\n" +
                "<b>R</b> : Change traversal state modifier to Revealed\n" +
                "<b>N</b> : Clear traversal state modifier\n" +
                "<b>O (path)</b> : Change path orientation", 
                myStyle
            );
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(EditableLevel);
        }

        Repaint();
    }

    public Rect DrawContainerRect(Rect containerRect)
    {
        var originalContainerRect = containerRect;

        var xWidth = Mathf.Min(containerRect.width / Mathf.Max(1, EditableLevel.KWidth), containerRect.height / Mathf.Max(1, EditableLevel.KWidth));

        EditorGUI.DrawRect(containerRect, new Color(49f / 255f, 77f / 255f, 121f / 255f));

        for (var x = 0; x < EditableLevel.KWidth; x++)
        {
            for (var y = 0; y < EditableLevel.KHeight; y++)
            {
                var element = EditableLevel.GetElement(x, y);
                var newRect = new Rect(containerRect.x + xWidth * x, containerRect.y + xWidth * y, xWidth, xWidth);

                var currentX = x;
                var currentY = y;

                var currentEvent = Event.current;

                if (!string.IsNullOrEmpty(element))
                {
                    var sprite = (Sprite)null;
                    var additionalSprite = (Sprite)null;
                    
                    if (EditableLevel.ElementContains(x, y, EditableLevel.KNode))
                    {
                        // Start node
                        if (x == (int)EditableLevel.StartNode.x && y == (int)EditableLevel.StartNode.y)
                        {
                            sprite = EditableLevel.LevelPrefab.GetComponent<Level>().NodePrefab.GetComponent<Node>().StartSprite;
                        }
                        // End node
                        else if (x == (int)EditableLevel.EndNode.x && y == (int)EditableLevel.EndNode.y)
                        {
                            sprite = EditableLevel.LevelPrefab.GetComponent<Level>().NodePrefab.GetComponent<Node>().EndSprite;
                        }
                        // Node
                        else
                        {
                            sprite = EditableLevel.LevelPrefab.GetComponent<Level>().NodePrefab.GetComponent<Node>().Sprite;

                            // Collectables

                            // Key
                            if (EditableLevel.ElementContains(x, y, EditableLevel.KKey))
                            {
                                var keyPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().KeyPrefab.GetComponent<Key>();
                                
                                if (EditableLevel.ElementContains(x, y, EditableLevel.KColorRed))
                                {
                                    additionalSprite = keyPrefab.EditorSprites.RedSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorBlue))
                                {
                                    additionalSprite = keyPrefab.EditorSprites.BlueSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorGreen))
                                {
                                    additionalSprite = keyPrefab.EditorSprites.GreenSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorOrange))
                                {
                                    additionalSprite = keyPrefab.EditorSprites.OrangeSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorPurple))
                                {
                                    additionalSprite = keyPrefab.EditorSprites.PurpleSprite;
                                }
                            }
                            // Link
                            if (EditableLevel.ElementContains(x, y, EditableLevel.KLink))
                            {
                                var LinkPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().LinkPrefab.GetComponent<Link>();

                                if (EditableLevel.ElementContains(x, y, EditableLevel.KColorRed))
                                {
                                    additionalSprite = LinkPrefab.EditorSprites.RedSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorBlue))
                                {
                                    additionalSprite = LinkPrefab.EditorSprites.BlueSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorGreen))
                                {
                                    additionalSprite = LinkPrefab.EditorSprites.GreenSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorOrange))
                                {
                                    additionalSprite = LinkPrefab.EditorSprites.OrangeSprite;
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorPurple))
                                {
                                    additionalSprite = LinkPrefab.EditorSprites.PurpleSprite;
                                }
                            }
                            // Flag
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KFlag))
                            {
                                var flagPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().FlagPrefab.GetComponent<Flag>();

                                additionalSprite = flagPrefab.GetComponent<SpriteRenderer>().sprite;
                            }
                        }
                    }
                    else if (EditableLevel.ElementContains(x, y, EditableLevel.KPath))
                    {
                        // Horizontal path
                        if (EditableLevel.ElementContains(x, y, EditableLevel.KHorizontal))
                        {
                            sprite = EditableLevel.LevelPrefab.GetComponent<Level>().HorizontalPathPrefab.GetComponent<HorizontalPath>().EditorSprite;
                        }
                        // Vertical path
                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KVertical))
                        {
                            sprite = EditableLevel.LevelPrefab.GetComponent<Level>().VerticalPathPrefab.GetComponent<VerticalPath>().EditorSprite;
                        }

                        // Obstacles

                        // Lock
                        if (EditableLevel.ElementContains(x, y, EditableLevel.KLock))
                        {
                            var lockPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().LockPrefab.GetComponent<Lock>();

                            if (EditableLevel.ElementContains(x, y, EditableLevel.KColorRed))
                            {
                                additionalSprite = lockPrefab.EditorSprites.RedSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorBlue))
                            {
                                additionalSprite = lockPrefab.EditorSprites.BlueSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorGreen))
                            {
                                additionalSprite = lockPrefab.EditorSprites.GreenSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorOrange))
                            {
                                additionalSprite = lockPrefab.EditorSprites.OrangeSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KColorPurple))
                            {
                                additionalSprite = lockPrefab.EditorSprites.PurpleSprite;
                            }
                        }
                        // Wall
                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KWall))
                        {
                            var wallPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().WallPrefab.GetComponent<Wall>();
                            
                            if (EditableLevel.ElementContains(x, y, EditableLevel.KHorizontal))
                            {
                                additionalSprite = wallPrefab.EditorSprites.VerticalSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KVertical))
                            {
                                additionalSprite = wallPrefab.EditorSprites.HorizontalSprite;
                            }

                            
                        }
                        // Crack
                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KCrack))
                        {
                            var crackPrefab = EditableLevel.LevelPrefab.GetComponent<Level>().CrackPrefab.GetComponent<Crack>();

                            additionalSprite = crackPrefab.GetComponent<SpriteRenderer>().sprite;
                        }
                        // Slide
                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KSlide))
                        {
                            var slidePrefab = EditableLevel.LevelPrefab.GetComponent<Level>().SlidePrefab.GetComponent<Slide>();

                            if (EditableLevel.ElementContains(x, y, EditableLevel.KDirectionUp))
                            {
                                additionalSprite = slidePrefab.EditorSprites.UpSprite;
                            }
                            else if(EditableLevel.ElementContains(x, y, EditableLevel.KDirectionRight))
                            {
                                additionalSprite = slidePrefab.EditorSprites.RightSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KDirectionDown))
                            {
                                additionalSprite = slidePrefab.EditorSprites.DownSprite;
                            }
                            else if (EditableLevel.ElementContains(x, y, EditableLevel.KDirectionLeft))
                            {
                                additionalSprite = slidePrefab.EditorSprites.LeftSprite;
                            }
                        }
                    }

                    if (sprite != null)
                    {
                        DrawSprite(sprite, newRect);
                    }

                    if (additionalSprite != null)
                    {
                        var additionalRect = new Rect(containerRect.x + xWidth * x + 4, containerRect.y + xWidth * y + 4, 16, 16);
                        DrawSprite(additionalSprite, additionalRect);
                    }

                    // Traversal state
                    if (EditableLevel.ElementContains(x, y, EditableLevel.KTraversalStateCovered))
                    {
                        DrawSprite(EditableLevel.LevelPrefab.GetComponent<Level>().CoveredPrefab.GetComponent<Covered>().EditorSprite, newRect);
                    }
                    else if(EditableLevel.ElementContains(x, y, EditableLevel.KTraversalStateRevealed))
                    {
                        DrawSprite(EditableLevel.LevelPrefab.GetComponent<Level>().RevealedPrefab.GetComponent<Revealed>().EditorSprite, newRect);
                    }
                }

                if (newRect.Contains(currentEvent.mousePosition))
                {
                    EditorGUI.DrawRect(newRect, new Color(0, 0, 0, 0.25f));

                    HandleShortcuts(currentEvent, currentX, currentY);
                    
                    if (currentEvent.type == EventType.MouseDown && currentEvent.isMouse)
                    {
                        switch (currentEvent.button)
                        {
                            case 0:
                                
                                if (EditableLevel.ElementContains(x, y, EditableLevel.KPath))
                                {
                                    if (EditableLevel.ElementContains(x, y, EditableLevel.Obstacles))
                                    {
                                        if (EditableLevel.ElementContains(x, y, EditableLevel.Directions))
                                        {
                                            CycleDirections(x, y);
                                        }
                                        else if(EditableLevel.ElementContains(x, y, EditableLevel.Colors))
                                        {
                                            CycleColors(x, y);
                                        }
                                    }
                                    else if ((x + y) % 2 == 1 && !EditableLevel.Shifted || (x + y) % 2 == 0 && EditableLevel.Shifted)
                                    {
                                        ChangePathOrientation(currentX, currentY);
                                    }
                                }
                                else if (EditableLevel.ElementContains(x, y, EditableLevel.KNode))
                                {
                                    if (EditableLevel.ElementContains(x, y, EditableLevel.Collectables))
                                    {
                                        if (EditableLevel.ElementContains(x, y, EditableLevel.Colors))
                                        {
                                            CycleColors(x, y);
                                        }
                                    }
                                }
                                else
                                {
                                    if ((x + y) % 2 == 1 && !EditableLevel.Shifted || (x + y) % 2 == 0 && EditableLevel.Shifted)
                                    {
                                        SetElement(x, y, EditableLevel.KPath);

                                        var upIsNode = y < EditableLevel.KHeight - 1 && EditableLevel.ElementContains(x, y + 1, EditableLevel.KNode);
                                        var downIsNode = y > 0 && EditableLevel.ElementContains(x, y - 1, EditableLevel.KNode);

                                        if (upIsNode || downIsNode)
                                        {
                                            AddToElement(x, y, EditableLevel.KVertical);
                                        }
                                        else
                                        {
                                            AddToElement(x, y, EditableLevel.KHorizontal);
                                        }
                                    }
                                    else
                                    {
                                        SetElement(x, y, EditableLevel.KNode);
                                    }
                                }

                                EditorUtility.SetDirty(EditableLevel);

                                break;

                            case 1:
                                
                                GenericMenu menu = new GenericMenu();

                                // Traversal state menu
                                menu.AddItem(new GUIContent("Traversal State/Normal"), false, () =>
                                {
                                    ChangeTraversalState(currentX, currentY, string.Empty);
                                });
                                menu.AddItem(new GUIContent("Traversal State/Covered"), false, () =>
                                {
                                    ChangeTraversalState(currentX, currentY, EditableLevel.KTraversalStateCovered);
                                });
                                menu.AddItem(new GUIContent("Traversal State/Revealed"), false, () =>
                                {
                                    ChangeTraversalState(currentX, currentY, EditableLevel.KTraversalStateRevealed);
                                });
                                
                                if (EditableLevel.ElementContains(x, y, EditableLevel.KNode) || EditableLevel.ElementContains(x, y, EditableLevel.KPath))
                                {
                                    if ((x + y) % 2 == 1 && !EditableLevel.Shifted || (x + y) % 2 == 0 && EditableLevel.Shifted)
                                    {
                                        // Path menu
                                        menu.AddItem(new GUIContent("Change Orientation"), false, () =>
                                        {
                                            ChangePathOrientation(currentX, currentY);
                                        });
                                        menu.AddItem(new GUIContent("Obstacle/Lock"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KLock);
                                            ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                        });
                                        menu.AddItem(new GUIContent("Obstacle/Wall"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KWall);
                                        });
                                        menu.AddItem(new GUIContent("Obstacle/Crack"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KCrack);
                                        });
                                        menu.AddItem(new GUIContent("Obstacle/Slide"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KSlide);
                                            ChangeDirection(currentX, currentY, EditableLevel.KDirectionUp);
                                        });
                                        menu.AddItem(new GUIContent("Obstacle/None"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, string.Empty);
                                        });
                                        
                                        // Lock submenu
                                        if (EditableLevel.ElementContains(x, y, EditableLevel.KLock))
                                        {
                                            menu.AddItem(new GUIContent("Lock/Color/Red"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                            });
                                            menu.AddItem(new GUIContent("Lock/Color/Green"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorGreen);
                                            });
                                            menu.AddItem(new GUIContent("Lock/Color/Blue"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorBlue);
                                            });
                                            menu.AddItem(new GUIContent("Lock/Color/Orange"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorOrange);
                                            });
                                            menu.AddItem(new GUIContent("Lock/Color/Purple"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorPurple);
                                            });
                                            menu.AddItem(new GUIContent("Lock/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                        // Wall submenu
                                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KWall))
                                        {
                                            menu.AddItem(new GUIContent("Wall/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                        // Crack submenu
                                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KCrack))
                                        {
                                            menu.AddItem(new GUIContent("Crack/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                        // Slide submenu
                                        else if(EditableLevel.ElementContains(x, y, EditableLevel.KSlide))
                                        {
                                            menu.AddItem(new GUIContent("Slide/Direction/Up"), false, () =>
                                            {
                                                ChangeDirection(currentX, currentY, EditableLevel.KDirectionUp);
                                            });
                                            menu.AddItem(new GUIContent("Slide/Direction/Right"), false, () =>
                                            {
                                                ChangeDirection(currentX, currentY, EditableLevel.KDirectionRight);
                                            });
                                            menu.AddItem(new GUIContent("Slide/Direction/Down"), false, () =>
                                            {
                                                ChangeDirection(currentX, currentY, EditableLevel.KDirectionDown);
                                            });
                                            menu.AddItem(new GUIContent("Slide/Direction/Left"), false, () =>
                                            {
                                                ChangeDirection(currentX, currentY, EditableLevel.KDirectionLeft);
                                            });
                                            menu.AddItem(new GUIContent("Slide/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // Node menu
                                        menu.AddItem(new GUIContent("Collectable/Key"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KKey);
                                            ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                        });
                                        menu.AddItem(new GUIContent("Collectable/Link"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KLink);
                                            ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                        });
                                        menu.AddItem(new GUIContent("Collectable/Flag"), false, () =>
                                        {
                                            ChangeContent(currentX, currentY, EditableLevel.KFlag);
                                        });
                                        menu.AddItem(new GUIContent("Collectable/None"), false, () =>
                                        {
                                            RemoveAllContent(currentX, currentY);
                                        });
                                        menu.AddItem(new GUIContent("Make Start Node"), false, () =>
                                        {
                                            MakeStartNode(currentX, currentY);
                                        });
                                        menu.AddItem(new GUIContent("Make End Node"), false, () =>
                                        {
                                            MakeEndNode(currentX, currentY);
                                        });

                                        // Key submenu
                                        if (EditableLevel.ElementContains(x, y, EditableLevel.KKey))
                                        {
                                            menu.AddItem(new GUIContent("Key/Color/Red"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                            });
                                            menu.AddItem(new GUIContent("Key/Color/Green"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorGreen);
                                            });
                                            menu.AddItem(new GUIContent("Key/Color/Blue"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorBlue);
                                            });
                                            menu.AddItem(new GUIContent("Key/Color/Orange"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorOrange);
                                            });
                                            menu.AddItem(new GUIContent("Key/Color/Purple"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorPurple);
                                            });
                                            menu.AddItem(new GUIContent("Key/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                        // Link submenu
                                        if (EditableLevel.ElementContains(x, y, EditableLevel.KLink))
                                        {
                                            menu.AddItem(new GUIContent("Link/Color/Red"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorRed);
                                            });
                                            menu.AddItem(new GUIContent("Link/Color/Green"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorGreen);
                                            });
                                            menu.AddItem(new GUIContent("Link/Color/Blue"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorBlue);
                                            });
                                            menu.AddItem(new GUIContent("Link/Color/Orange"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorOrange);
                                            });
                                            menu.AddItem(new GUIContent("Link/Color/Purple"), false, () =>
                                            {
                                                ChangeColor(currentX, currentY, EditableLevel.KColorPurple);
                                            });
                                            menu.AddItem(new GUIContent("Link/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                        // Flag submenu
                                        else if (EditableLevel.ElementContains(x, y, EditableLevel.KFlag))
                                        {
                                            menu.AddItem(new GUIContent("Flag/Delete"), false, () =>
                                            {
                                                ChangeContent(currentX, currentY, string.Empty);
                                            });
                                        }
                                    }

                                    menu.AddItem(new GUIContent("Delete Element"), false, () =>
                                    {
                                        DeleteElement(currentX, currentY);
                                    });
                                    menu.AddItem(new GUIContent("Clear"), false, () =>
                                    {
                                        ClearElement(currentX, currentY);
                                    });
                                    
                                    currentEvent.Use();
                                }

                                menu.ShowAsContext();

                                EditorUtility.SetDirty(EditableLevel);

                                break;
                        }
                    }
                }
            }
        }

        return new Rect(originalContainerRect.x, originalContainerRect.y, originalContainerRect.width, EditorGUIUtility.singleLineHeight + (EditableLevel.KHeight * xWidth));
    }

    private void DrawSprite(Sprite sprite, Rect rect)
    {
        var spriteRect = rect;

        var fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
        var size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

        var coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        var ratio = Vector2.zero;
        ratio.x = spriteRect.width / size.x;
        ratio.y = spriteRect.height / size.y;
        var minRatio = Mathf.Min(ratio.x, ratio.y);

        var center = spriteRect.center;
        spriteRect.width = size.x * minRatio;
        spriteRect.height = size.y * minRatio;
        spriteRect.center = center;

        GUI.DrawTextureWithTexCoords(spriteRect, sprite.texture, coords);
    }

    private void DeleteInstance()
    {
        var levels = FindObjectsOfType<Level>();
        foreach (var level in levels)
        {
            if (level.gameObject.name == EditableLevel.name)
            {
                DestroyImmediate(level.gameObject);
            }
        }
    }

    private void InstantiateLevel()
    {
        DeleteInstance();
        EditableLevel.InstantiateLevel();
    }

    private void ResetSolution()
    {
        EditableLevel.NumberOfSolutions = -1;
        EditableLevel.MinimumMoves = -1;
        EditableLevel.MinimumMovesWithFlag = -1;
    }

    public static void ComputeSolution(EditableLevel editableLevel)
    {
        Solutions = new List<HypotheticalSolution>();

        var firstSolution = new HypotheticalSolution(editableLevel);
        firstSolution.Solve();

        var minimumMoves = int.MaxValue;
        var minimumMovesWithFlag = int.MaxValue;

        for (var i = 0; i < Solutions.Count; i++)
        {
            if (Solutions[i].CollectedCollectables.Contains(EditableLevel.KFlag) && Solutions[i].Movements.Count < minimumMovesWithFlag)
            {
                var lingeringFlag = false;

                for (var j = 0; j < Solutions[i].Elements.Length; j++)
                {
                    var element = Solutions[i].Elements[j];

                    if (element != null && element.Contains(EditableLevel.KFlag))
                    {
                        lingeringFlag = true;
                    }
                }

                if (!lingeringFlag)
                {
                    minimumMovesWithFlag = Solutions[i].Movements.Count;
                }
            }
            if (Solutions[i].Movements.Count < minimumMoves)
            {
                minimumMoves = Solutions[i].Movements.Count;
            }
        }

        editableLevel.NumberOfSolutions = Solutions.Count;
        editableLevel.MinimumMoves = minimumMoves / 2;
        editableLevel.MinimumMovesWithFlag = minimumMovesWithFlag / 2;

        var numberOfElements = 0;
        var numberOfFlags = 0;
        string[] mechanics = {
            EditableLevel.KSlide, 
            EditableLevel.KWall, 
            EditableLevel.KCrack, 
            EditableLevel.KLock, 
            EditableLevel.KLink,
            EditableLevel.KTraversalStateCovered, 
            EditableLevel.KTraversalStateRevealed
        };

        bool[] differentMechanics = new bool[mechanics.Length];
        var numberOfDifferentMechanics = 0;

        for (var i = 0; i < editableLevel.Elements.Length; i++)
        {
            if (!string.IsNullOrEmpty(editableLevel.Elements[i]))
            {
                numberOfElements++;

                if (editableLevel.Elements[i].Contains(EditableLevel.KFlag))
                {
                    numberOfFlags++;
                }

                for (var j = 0; j < mechanics.Length; j++)
                {
                    if (!differentMechanics[j] && editableLevel.Elements[i].Contains(mechanics[j]))
                    {
                        differentMechanics[j] = true;
                        numberOfDifferentMechanics++;
                    }
                }
            }
        }

        editableLevel.Difficulty =
            numberOfDifferentMechanics * 10 +
            editableLevel.MinimumMoves * 5 +
            editableLevel.MinimumMovesWithFlag * 5 +
            numberOfFlags * 10 +
            numberOfElements;

        var newName = string.Format("{0}-{1}-{2}-{3}-{4}",
            numberOfDifferentMechanics,
            (editableLevel.MinimumMoves + editableLevel.MinimumMovesWithFlag).ToString("D3"), 
            numberOfFlags,
            numberOfElements.ToString("D3"),
            editableLevel.NumberOfSolutions.ToString("D4")
        );

        var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        AssetDatabase.RenameAsset(assetPath, newName);
        AssetDatabase.SaveAssets();
    }

    private string GetSuggestedTime()
    {
        if (EditableLevel.MinimumMoves < 0)
        {
            return "?";
        }

        return GetSuggestedTimeValue(EditableLevel).ToString();
    }

    private string GetSuggestedMoves()
    {
        if (EditableLevel.MinimumMoves < 0)
        {
            return "?";
        }

        return GetSuggestedMovesValue(EditableLevel).ToString();
    }

    private static int GetSuggestedTimeValue(EditableLevel editableLevel)
    {
        return Mathf.CeilToInt(editableLevel.MinimumMoves * 0.5f);
    }

    private static int GetSuggestedMovesValue(EditableLevel editableLevel)
    {
        return editableLevel.MinimumMoves + 1;
    }

    private void HandleShortcuts(Event currentEvent, int x, int y)
    {
        // GetKey
        if (currentEvent.keyCode == KeyCode.N)
        {
            ChangeTraversalState(x, y, string.Empty);
        }
        if (currentEvent.keyCode == KeyCode.C)
        {
            ChangeTraversalState(x, y, EditableLevel.KTraversalStateCovered);
        }
        if (currentEvent.keyCode == KeyCode.R)
        {
            ChangeTraversalState(x, y, EditableLevel.KTraversalStateRevealed);
        }
        if (currentEvent.keyCode == KeyCode.Backspace)
        {
            ClearElement(x, y);
        }
        //

        if (_shortcutKeyReleased)
        {
            // GetKeyDown
            if (currentEvent.keyCode == KeyCode.O)
            {
                ChangePathOrientation(x, y);
            }
            //

            if (currentEvent.keyCode != KeyCode.None)
            {
                _shortcutKeyReleased = false;
            }
        }
        
        if (currentEvent.type == EventType.KeyUp)
        {
            _shortcutKeyReleased = true;
        }
    }

    private int GetNextValueIndex(int x, int y, string[] values)
    {
        var currentValueIndex = 0;

        for (var i = 0; i < values.Length; i++)
        {
            if (EditableLevel.ElementContains(x, y, values[i]))
            {
                currentValueIndex = i;
                break;
            }
        }

        currentValueIndex++;

        if (currentValueIndex >= values.Length)
        {
            currentValueIndex = 0;
        }

        return currentValueIndex;
    }

    private void CycleColors(int x, int y)
    {
        var nextValueIndex = GetNextValueIndex(x, y, EditableLevel.Colors);
        ChangeColor(x, y, EditableLevel.Colors[nextValueIndex]);
    }

    private void CycleDirections(int x, int y)
    {
        var nextValueIndex = GetNextValueIndex(x, y, EditableLevel.Directions);
        ChangeDirection(x, y, EditableLevel.Directions[nextValueIndex]);
    }

    #region Undo-able functions

    private void ShiftLevel(ShiftDirection direction)
    {
        Undo.RecordObject(EditableLevel, "ShiftLevel");

        switch (direction)
        {
            case ShiftDirection.Up:

                for (var y = 0; y < EditableLevel.KHeight; y++)
                {
                    for (var x = 0; x < EditableLevel.KWidth; x++)
                    {
                        if (string.IsNullOrEmpty(EditableLevel.GetElement(x, y)))
                        {
                            continue;
                        }

                        if (y == 0)
                        {
                            return;
                        }

                        EditableLevel.SetElement(x, y - 1, EditableLevel.GetElement(x, y));
                        EditableLevel.SetElement(x, y, null);
                    }
                }

                EditableLevel.StartNode = new Vector2(EditableLevel.StartNode.x, EditableLevel.StartNode.y - 1);
                EditableLevel.EndNode = new Vector2(EditableLevel.EndNode.x, EditableLevel.EndNode.y - 1);

                break;

            case ShiftDirection.Right:

                for (var x = EditableLevel.KWidth - 1; x >= 0; x--)
                {
                    for (var y = 0; y < EditableLevel.KHeight; y++)
                    {
                        if (string.IsNullOrEmpty(EditableLevel.GetElement(x, y)))
                        {
                            continue;
                        }

                        if (x == EditableLevel.KWidth - 1)
                        {
                            return;
                        }

                        EditableLevel.SetElement(x + 1, y, EditableLevel.GetElement(x, y));
                        EditableLevel.SetElement(x, y, null);
                    }
                }

                EditableLevel.StartNode = new Vector2(EditableLevel.StartNode.x + 1, EditableLevel.StartNode.y);
                EditableLevel.EndNode = new Vector2(EditableLevel.EndNode.x + 1, EditableLevel.EndNode.y);

                break;

            case ShiftDirection.Down:

                for (var y = EditableLevel.KHeight - 1; y >= 0; y--)
                {
                    for (var x = 0; x < EditableLevel.KWidth; x++)
                    {
                        if (string.IsNullOrEmpty(EditableLevel.GetElement(x, y)))
                        {
                            continue;
                        }

                        if (y == EditableLevel.KHeight - 1)
                        {
                            return;
                        }

                        EditableLevel.SetElement(x, y + 1, EditableLevel.GetElement(x, y));
                        EditableLevel.SetElement(x, y, null);
                    }
                }

                EditableLevel.StartNode = new Vector2(EditableLevel.StartNode.x, EditableLevel.StartNode.y + 1);
                EditableLevel.EndNode = new Vector2(EditableLevel.EndNode.x, EditableLevel.EndNode.y + 1);

                break;

            case ShiftDirection.Left:

                for (var x = 0; x < EditableLevel.KWidth; x++)
                {
                    for (var y = 0; y < EditableLevel.KHeight; y++)
                    {
                        if (string.IsNullOrEmpty(EditableLevel.GetElement(x, y)))
                        {
                            continue;
                        }

                        if (x == 0)
                        {
                            return;
                        }

                        EditableLevel.SetElement(x - 1, y, EditableLevel.GetElement(x, y));
                        EditableLevel.SetElement(x, y, null);
                    }
                }

                EditableLevel.StartNode = new Vector2(EditableLevel.StartNode.x - 1, EditableLevel.StartNode.y);
                EditableLevel.EndNode = new Vector2(EditableLevel.EndNode.x - 1, EditableLevel.EndNode.y);

                break;
        }

        EditableLevel.Shifted = !EditableLevel.Shifted;
    }

    private void SetElement(int x, int y, string element)
    {
        Undo.RecordObject(EditableLevel, "SetElement");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.LevelElements);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Obstacles);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Collectables);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Colors);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Directions);
        EditableLevel.AddToElement(x, y, element);
    }

    private void DeleteElement(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "DeleteElement");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.LevelElements);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Obstacles);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Collectables);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Colors);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Directions);
    }

    private void AddToElement(int x, int y, string content)
    {
        Undo.RecordObject(EditableLevel, "AddToElement");
        ResetSolution();

        EditableLevel.AddToElement(x, y, content);
    }

    private void MakeStartNode(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "MakeStartNode");
        ResetSolution();

        EditableLevel.StartNode = new Vector2(x, y);
    }

    private void MakeEndNode(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "MakeEndNode");
        ResetSolution();

        EditableLevel.EndNode = new Vector2(x, y);
    }

    private void RemoveAllContent(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "RemoveAllContent");
        ResetSolution();

        EditableLevel.RemoveAllContent(x, y);
    }

    private void ChangeContent(int x, int y, string content)
    {
        Undo.RecordObject(EditableLevel, "ChangeContent");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.Obstacles);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Collectables);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Colors);
        EditableLevel.RemoveFromElement(x, y, EditableLevel.Directions);
        EditableLevel.AddToElement(x, y, content);
    }

    private void ChangeColor(int x, int y, string color)
    {
        Undo.RecordObject(EditableLevel, "ChangeColor");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.Colors);
        EditableLevel.AddToElement(x, y, color);
    }

    private void ChangeDirection(int x, int y, string direction)
    {
        Undo.RecordObject(EditableLevel, "ChangeDirection");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.Directions);
        EditableLevel.AddToElement(x, y, direction);
    }

    private void ChangeTraversalState(int x, int y, string traversalState)
    {
        Undo.RecordObject(EditableLevel, "ChangeTraversalState");
        ResetSolution();

        EditableLevel.RemoveFromElement(x, y, EditableLevel.TraversalStates);
        EditableLevel.AddToElement(x, y, traversalState);
    }

    private void ChangePathOrientation(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "ChangePathOrientation");
        ResetSolution();

        var element = EditableLevel.GetElement(x, y);
        var newElement = new char[element.Length];

        for (var i = 0; i < element.Length; i++)
        {
            if (element[i] == 'V')
            {
                newElement[i] = 'H';
            }
            else if (element[i] == 'H')
            {
                newElement[i] = 'V';
            }
            else
            {
                newElement[i] = element[i];
            }
        }

        EditableLevel.SetElement(x, y, new string(newElement));
    }

    private void ClearElement(int x, int y)
    {
        Undo.RecordObject(EditableLevel, "ClearElement");
        ResetSolution();

        EditableLevel.SetElement(x, y, null);
    }

    private void ClearLevel()
    {
        Undo.RecordObject(EditableLevel, "ClearLevel");
        ResetSolution();

        for (var x = 0; x < EditableLevel.KWidth; x++)
        {
            for (var y = 0; y < EditableLevel.KHeight; y++)
            {
                ClearElement(x, y);
            }
        }

        EditableLevel.StartNode = Vector2.zero;
        EditableLevel.EndNode = Vector2.zero;
    }

    public static void ApplySuggestedExpectedValues(EditableLevel editableLevel)
    {
        if (editableLevel.ExpectedMoves < 0)
        {
            ComputeSolution(editableLevel);
        }

        Undo.RecordObject(editableLevel, "ApplySuggestedExpectedValues");

        editableLevel.ExpectedMoves = GetSuggestedMovesValue(editableLevel);
        editableLevel.ExpectedTime = GetSuggestedTimeValue(editableLevel);
    }
    
    #endregion
}