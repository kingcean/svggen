# SVG Icon Generator by AI

An open-source project dedicated to generating SVG icons for common design and development scenarios.
Users describe the icons they need in natural language and use a large language model they provide to turn their ideas into scalable SVG icons.

## Use Cases

- **Websites and application interfaces**: Create icons for navigation, buttons, menus, and status indicators.
- **Product prototypes and design exploration**: Generate icons to communicate and validate design ideas.
- **Documentation and presentations**: Add simple visual elements to documentation, slides, and other content.

## Usage

1. **Provide a large language model**: Supply and configure your own large language model. You are responsible for access to the model service and any associated costs.
2. **Describe your icon**: Specify the icon you want to generate, optionally including its intended use, style, colors, and dimensions.

SVG is a vector graphics format that scales across different sizes and resolutions and is easy to integrate into websites and applications.
The quality of generated results depends on the selected model and your description. Review both the visual output and SVG content before use.

## Build

You can build and run the project using the .NET CLI or Visual Studio.

To build with the .NET CLI, install the .NET 10 SDK and run the following command from the repository root:

```sh
dotnet build SvgGenerator.slnx
```

Before running the application, configure the required settings, such as the large language model gateway, through environment variables or a `config.json` file.

## License

This project is [MIT licensed](./LICENSE).

Ownership of and responsibility for icons generated with this tool belong entirely to the person generating them, subject to any overriding terms of the large language model service used or applicable local laws and regulations.
The owners and maintainers of this tool do not acquire any ownership rights in generated icons merely by providing the tool.
