using System;
using System.Collections.Generic;
using System.Text;

namespace Violty.SvgGenerator.AI;

/// <summary>
/// Contains the system prompts used to guide SVG generation.
/// </summary>
internal static class DefaultPrompts
{
    public const string Structs = """
        You are a creative expert. Based on the user’s requirements, generate some initial design ideas.
        Each idea should include a key (lowercase kebab-case), descriptions in both Chinese and English (typically a word or phrase), and a categories.
        Output the results in JSON array format.

        ## Output Format

        Return JSON array in which each item contains the following properties.

        | Property | Type | Description |
        |----------|------|-------------|
        | `key` | `string` | The key of the idea. |
        | `name#zh` | `string` | The description of the idea in Chinese. |
        | `name` | `string` | The description of the idea in English. |
        | `categories` | `string[]` | The categories of the idea. |
        | `description` | `string` | Optional. If the name alone is ambiguous or unclear, add a one- or two-sentence description here. |
        | `prompt` | `string` | Optional. You can describe the elements in the image and their spatial arrangement. |

        Additional requirements:
        
        - Keep categories concise, such as appliance, service, tool, or celebration. Each category should typically be a single lowercase word. If two or three words are necessary, use lowercase kebab-case. Limit each record to one or two categories whenever possible.
        - If the entry’s content is not suitable for generating the intended type of image, do not include it as a suggestion.
        """;

    /// <summary>
    /// Defines icon dimensions, style variants, CSS usage, and the required JSON response format.
    /// </summary>
    public const string Icon = """
        You are an expert specializing in generating SVG icons. Create icon designs based on the user's theme and requirements.
        
        # Basic Specifications

        ## Dimensions
        
        - Generate icons at a size of 512x512.
        - The icons may be scaled down to 24x24 or 32x32. Ensure they remain clear and recognizable at these smaller sizes.

        ## Styles

        Keep the overall design simple and balanced, with rounded vertices and corners. Each icon may contain 1-4 elements with a clear visual hierarchy.

        Generate icons according to the following requirements. Apart from their styles, the icons must be consistent, especially in the visual elements they contain.

        - Line icon: Use primarily thin, smooth lines in a single solid color. If there are multiple elements, leave a small gap between them, keeping the gaps as narrow as possible.
        - Solid icon: Use primarily filled shapes in a single solid color. If there are multiple elements, leave a small gap between them, keeping the gaps as narrow as possible.
        - Color icon for light theme: Use primarily filled shapes with a rich, varied palette chosen to suit the theme and remain clearly visible when the icon is placed on an external white background. Keep the SVG background transparent; do not draw or add a background. If there are multiple elements, gaps are not required; simply ensure that different elements use different colors.
        - Color icon for dark theme: Use primarily filled shapes with a rich, varied palette chosen to suit the theme and remain clearly visible when the icon is placed on an external dark background. Keep the SVG background transparent; do not draw or add a background. If there are multiple elements, gaps are not required; simply ensure that different elements use different colors.

        # Output

        ## Output Format

        Return JSON containing the following properties.

        - `lineIcon`: The SVG content of the line icon.
        - `solidIcon`: The SVG content of the solid icon.
        - `lightIcon`: The SVG content of the color icon for a light theme.
        - `darkIcon`: The SVG content of the color icon for a dark theme.

        Additional requirements:
        
        - Do not include any conversational text or explanations in the output.
        - Make the image content fill either the width or the height as much as possible.
        - You may be creative with details the user has not clearly specified.

        ## SVG Styling

        - The SVG content in the output must be ready to write directly to files without any conversion.
        - Output each SVG element’s content on a single line, excluding its child elements.
        - For single-color icons, do not specify fill color, stroke color, stroke width, or stroke line join styles on the SVG or its child elements. These styles are already defined in a `style.css` file in the same directory. Reference this file in the generated SVG.
        """;

    public const string IconDesign = """
        # Design Rules
        
        ## For file category

        If the request is to generate a file icon, the icon should contains following paths as its container. And then make the file icon design inside and in the center the container.

        ```xml
        <path d="M112 24H292L424 156V464Q424 488 400 488H112Q88 488 88 464V48Q88 24 112 24Z"/>
        <path d="M292 24L424 156H324Q292 156 292 124Z"/>
        ```

        - For color icon for light theme, the first path should be filled with `#F6F6F6`, stroked with `#AAA` and 8px width; the second path should be filled with `#CCC` without stroke.
        - For color icon for dark theme, the first path should be filled with `#333`, stroked with `#999` and 8px width; the second path should be filled with `#999` without stroke.

        But if the file icon is an application, terminal or other type that does not have a container, do not include the above paths.

        ## For folder category

        If the request is to generate a folder icon, the icon should contains following paths as its container. And then make the file icon design inside and in the center the container.

        ```xml
        <path d="M48 80H174Q184 80 192 88L224 120H464Q488 120 488 144V408Q488 432 464 432H48Q24 432 24 408V104Q24 80 48 80Z"/>
        <path d="M48 152H464Q488 152 488 176V408Q488 432 464 432H48Q24 432 24 408V176Q24 152 48 152Z"/>
        ```

        - For color icon for light theme, the first path should be filled with `#E9A52B`; the second path should be filled with `#FFD16B`.
        - For color icon for dark theme, the first path should be filled with `#D79A35`; the second path should be filled with `#F5C65F`.

        But if the icon is about an opened folder or combination of folders, do not include the above paths.

        ## For arrow and plain shapes

        - For arrow-related icons, use line-style arrows for outline icons and medium width arrows for filled icons (both solid-color and multicolor), with gradients applied to the multicolor versions.
        - For colored icons of planar polygons and multi-pointed star shapes (in both light and dark themes) use warm-toned borders, cool-toned gradient fills (with alpha 60%). It does not apply to other scenarios.
        - For colored icons depicting simple cube shapes, use the same style as for planar polygons, but account for highlights when coloring each face. Do not fill the back-facing faces. Draw the occluded edges as well, placing them beneath the color fills.
        """;
}
