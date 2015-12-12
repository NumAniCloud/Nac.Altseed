﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asd;
using Nac.Altseed.Input;
using Nac.Altseed.UI;

namespace Nac.Altseed.Test
{
    class SelectorTest : AltseedTest
    {
        class ListItem : TextObject2D, IActivatableSelectionItem
        {
            public ListItem()
            {
                Color = new Color(240, 196, 90, 255);
            }

            public void Activate()
            {
                Color = new Color(0, 0, 0, 255);
            }

            public void Disactivate()
            {
                Color = new Color(240, 196, 90, 255);
            }
        }

        Selector<int, int> selector;
        Font font;
        
        protected override void OnStart()
        {
            var controller = new KeyboardController<int>();
            controller.BindKey(Keys.Down, 0);
            controller.BindKey(Keys.Up, 1);
            controller.BindKey(Keys.Z, 2);
            controller.BindKey(Keys.X, 3);

            var layout = new LinearPanel()
            {
                ItemSpan = new Vector2DF(0, 36),
            };
            selector = new Selector<int, int>(controller, layout);
            selector.Texture = Engine.Graphics.CreateTexture2D("ListCursor.png");
            selector.BindKey(0, 1, 2, 3);
            Engine.AddObject2D(selector);
            selector.Loop = true;

            font = Engine.Graphics.CreateDynamicFont("", 20, new Color(255, 255, 255, 255), 0, new Color(0, 0, 0, 255));
            for(int i = 0; i < 8; i++)
            {
                var obj = new ListItem()
                {
                    Text = $"選択肢{i}",
                    Font = font,
                };
                Engine.AddObject2D(obj);
                selector.AddChoice(i, obj);
            }

            var moveSound = Engine.Sound.CreateSoundSource("kachi38.wav", true);
            var decideSound = Engine.Sound.CreateSoundSource("pi78.wav", true);
            var cancelSound = Engine.Sound.CreateSoundSource("pi11.wav", true);

            selector.OnSelectionChanged.Subscribe(i => Engine.Sound.Play(moveSound));
            selector.OnDecide.Subscribe(i =>
            {
                Engine.Sound.Play(decideSound);
            });
            selector.OnCancel.Subscribe(i =>
            {
                Engine.Sound.Play(cancelSound);
            });
        }

        protected override void OnUpdate()
        {
            if(Engine.Keyboard.GetKeyState(Keys.Q) == KeyState.Push)
            {
                var index = selector.ChoiceItems.Any() ? selector.ChoiceItems.Max(x => x.Choice) + 1 : 0;
                var obj = new ListItem()
                {
                    Font = font,
                    Text = $"追加アイテム{index}",
                };
                selector.AddChoice(index, obj);
                Engine.AddObject2D(obj);
            }
            else if(selector.ChoiceItems.Skip(3).Any() && Engine.Keyboard.GetKeyState(Keys.W) == KeyState.Push)
            {
                int index = selector.ChoiceItems.Max(x => x.Choice) + 1;
                var obj = new ListItem()
                {
                    Font = font,
                    Text = $"挿入アイテム{index}",
                };
                selector.InsertChoice(3, index, obj);
                Engine.AddObject2D(obj);
            }

            if(Engine.Keyboard.GetKeyState(Keys.LeftAlt) == KeyState.Hold)
            {
                if(Engine.Keyboard.GetKeyState(Keys.E) == KeyState.Push)
                {
                    var item = selector.ChoiceItems.Skip(3).FirstOrDefault()?.Choice;
                    if(item.HasValue)
                    {
                        selector.RemoveChoice(item.Value);
                    }
                }
                else if(Engine.Keyboard.GetKeyState(Keys.R) == KeyState.Push)
                {
                    selector.ClearChoice();
                }
            }
            else
            {
                if(Engine.Keyboard.GetKeyState(Keys.E) == KeyState.Push)
                {
                    selector.ChoiceItems.Skip(3).FirstOrDefault()?.Item.Vanish();
                }
                else if(Engine.Keyboard.GetKeyState(Keys.R) == KeyState.Push)
                {
                    foreach(var item in selector.ChoiceItems)
                    {
                        item.Item.Vanish();
                    }
                }
            }
        }
    }
}