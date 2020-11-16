using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace InlineMapping
{
	internal sealed class MappingInformation
	{
		// I have no idea why it's causing this to trip here, but not in my Rocks project
		// with the MockInformation type. I KNOW it's going to be non-null!
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public MappingInformation(ITypeSymbol sourceType, AttributeData attributeData) =>
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
			this.Validate(sourceType, attributeData);

		private void Validate(ITypeSymbol sourceType, AttributeData attributeData)
		{
			var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

			var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;

			if (!destinationType.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public && _.Parameters.Length == 0))
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoArgumentConstructorDescriptorConstants.Id, NoArgumentConstructorDescriptorConstants.Title,
					NoArgumentConstructorDescriptorConstants.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoArgumentConstructorDescriptorConstants.Id, NoArgumentConstructorDescriptorConstants.Title)),
					attributeData.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
			}

			var maps = ImmutableArray.CreateBuilder<string>();

			var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>()
				.Where(_ => _.SetMethod is not null && _.SetMethod.DeclaredAccessibility == Accessibility.Public).ToList();

			foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>()
				.Where(_ => _.GetMethod is not null && _.GetMethod.DeclaredAccessibility == Accessibility.Public))
			{
				var destinationProperty = destinationProperties.FirstOrDefault(
					_ => _.Name == sourceProperty.Name &&
						_.Type.Equals(sourceProperty.Type, SymbolEqualityComparer.Default) &&
						(sourceProperty.NullableAnnotation != NullableAnnotation.Annotated ||
							sourceProperty.NullableAnnotation == NullableAnnotation.Annotated && _.NullableAnnotation == NullableAnnotation.Annotated));

				if (destinationProperty is not null)
				{
					maps.Add($"{destinationProperty.Name} = self.{sourceProperty.Name},");
					destinationProperties.Remove(destinationProperty);
				}
				else
				{
					diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
						NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title,
						string.Format(CultureInfo.CurrentCulture, NoMatchDescriptorConstants.Message, sourceProperty.Name, "source", sourceType.Name),
						DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
						helpLinkUri: HelpUrlBuilder.Build(
							NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title)), null));
				}
			}

			foreach (var remainingDestinationProperty in destinationProperties)
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title,
					string.Format(CultureInfo.CurrentCulture, NoMatchDescriptorConstants.Message, remainingDestinationProperty.Name, "destination", destinationType.Name),
					DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title)), null));
			}

			if (maps.Count == 0)
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoPropertyMapsFoundDescriptorConstants.Id, NoPropertyMapsFoundDescriptorConstants.Title,
					NoPropertyMapsFoundDescriptorConstants.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoPropertyMapsFoundDescriptorConstants.Id, NoPropertyMapsFoundDescriptorConstants.Title)),
					attributeData.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
			}

			(this.SourceType, this.DestinationType, this.Maps, this.Diagnostics) =
				(sourceType, destinationType, maps.ToImmutable(), diagnostics.ToImmutable());
		}

		public ITypeSymbol DestinationType { get; private set; }
		public ImmutableArray<Diagnostic> Diagnostics { get; private set; }
		public ImmutableArray<string> Maps { get; private set; }
		public ITypeSymbol SourceType { get; private set; }
	}
}