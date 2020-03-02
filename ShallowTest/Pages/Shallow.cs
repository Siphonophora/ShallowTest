namespace ShallowTest.Pages
{
#line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using System.Net.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    using ShallowTest;
    using ShallowTest.Shared;
    using System.Reflection;
    using System.Diagnostics;

    public class Shallow : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            // First render out the childcontent to our own RenderTree
            var scaffold = new Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder();
            //scaffold.OpenComponent<ShallowTest.Shared.OuterComponent>(1);
            //scaffold.CloseComponent();
            scaffold.AddContent(1, ChildContent);

            // Now get the Append method through reflection because they mean and hid it
            MethodInfo? AddFrame = builder.GetType().GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic);

            // This will be used to track skipped frames
            int skipTo = 0;

            // Check each frame in the RenderTree
            foreach (var frame in scaffold.GetFrames().Array)
            {
                Debug.WriteLine($"Got Frame: {frame.Sequence} : {frame.FrameType} : {skipTo}");

                // GetFrames returns at least 16 frames, filled with FrameType None
                // but the renderer doesn't like them, so skip them or get errors
                if (frame.FrameType != Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.None)
                {
                    // This is how we shallow render - by skipping Component frames
                    if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Component)
                    {
                        // Set the next sequence we are going to allow to render
                        skipTo = frame.Sequence + frame.ComponentSubtreeLength;
                        
                        // debug - cos you wanna see it happen
                        Debug.WriteLine($"Skipping Component with length: {frame.ComponentSubtreeLength}");

                        // The renderer doesn't like it if we just drop the frame, so 
                        // for now I am replacing it with comment text saying the name of the component
                        // but you could do something different
                        for (int i = 0; i < frame.ComponentSubtreeLength; i++)
                        {
                            builder.AddMarkupContent(frame.Sequence + i, $"<{frame.ComponentType.Name}/><!-- {frame.ComponentType.Name}-BBBB-->");
                        }
                    }
                    else
                    {
                        if (frame.Sequence >= skipTo)
                        {
                            // Put the rendered frames we want on the actual RenderTree
                            AddFrame!.Invoke(builder, new object[] { frame });
                        }
                    }
                }
            }
        }

        [Parameter] public RenderFragment? ChildContent { 
            get;
            set; }
    }
}
