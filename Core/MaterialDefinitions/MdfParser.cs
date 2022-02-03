using System;
using System.IO;
using JetBrains.Annotations;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.MaterialDefinitions;

public class MdfParser
{
	private readonly string mFilename;
	private readonly string mContent;

	// The line last read using GetLine. Is trimmed of whitespace at start and end
	private string mLine;
	private int mLineNo = 0;

	private StringReader mIn;

	private bool mStrict = false;

	public MdfParser(string filename, string content)
	{
		mFilename = filename;
		mContent = content;
		mIn = new StringReader(content);
	}

	public MdfMaterial Parse()
	{
		var type = ParseMaterialType();

		switch (type) {
			case MdfType.Textured:
				return ParseTextured();
			case MdfType.General:
				return ParseGeneral();
			case MdfType.Clipper:
				return ParseClipper();
			default:
				throw CreateError($"Unrecognized MDF material type: {type}");
		}
	}

	public void SetStrict(bool strict)
	{
		mStrict = strict;
	}

	private bool GetLine()
	{
		mLine = mIn.ReadLine();
		if (mLine != null) {
			// Skip trailing windows newline characters in case the /
			// MDF file has been read in binary mode
			if (mLine.Length > 0 && mLine[mLine.Length - 1] == '\r')
			{
				mLine = mLine.Substring(0, mLine.Length - 1);
			}
			mLineNo++;
			return true;
		}
		return false;
	}

	private MdfType ParseMaterialType()
	{

		if (!GetLine()) {
			throw CreateError("File is empty");
		}

		var input = mLine.ToLowerInvariant();

		switch (input)
		{
			case "textured":
				return MdfType.Textured;
			case "general":
				return MdfType.General;
			case "clipper":
				return MdfType.Clipper;
			default:
				throw CreateError("Unrecognized material type '{0}'", input);
		}
	}

	private MdfMaterial ParseClipper()
	{

		var result = new MdfMaterial(MdfType.Clipper);

		result.enableZWrite = false;
		result.enableColorWrite = false;

		var tokenizer = new Tokenizer(mContent);
		tokenizer.NextToken(); // Skip material type

		while (tokenizer.NextToken()) {
			if (tokenizer.IsNamedIdentifier("wire")) {
				result.wireframe = true;
				result.enableColorWrite = true;
			} else if (tokenizer.IsNamedIdentifier("zfill")) {
				result.enableZWrite = true;
			} else if (tokenizer.IsNamedIdentifier("outline")) {
				result.outline = true;
				result.enableColorWrite = true;
			} else {
				if (mStrict) {
					throw CreateError("Unrecognized token '{0}'", tokenizer.TokenText.ToString());
				}
			}
		}

		return result;
	}

	private MdfMaterial ParseTextured()
	{

		var result = new MdfMaterial(MdfType.Textured);

		var tokenizer = new Tokenizer(mContent);
		/*
		    For some reason ToEE doesn't use the tokenizer for this
		    shader type normally. So we disable escape sequences to
		    get some form of compatibility.
		*/
		tokenizer.IsEnableEscapes = false;
		tokenizer.NextToken(); // Skip material type

		while (tokenizer.NextToken()) {
			if (tokenizer.IsNamedIdentifier("color")) {
				if (!ParseRgba(ref tokenizer, "Color", out result.diffuse)) {
					throw CreateError("Unable to parse diffuse color");
				}
			} else if (tokenizer.IsNamedIdentifier("texture")) {
				if (!tokenizer.NextToken() || !tokenizer.IsQuotedString) {
					throw CreateError("Missing filename for texture");
				}

				result.samplers[0].filename = tokenizer.TokenText.ToString();
			} else if (tokenizer.IsNamedIdentifier("colorfillonly")) {
				result.enableZWrite = false;
				result.enableColorWrite = true;
			} else if (tokenizer.IsNamedIdentifier("notlit")) {
				result.notLit = true;
			} else if (tokenizer.IsNamedIdentifier("notlite")) {
				// The original ToEE parser only does prefix parsing, which is why
				// "notlite" was accepted as "notlit".
				result.notLit = true;
			} else if (tokenizer.IsNamedIdentifier("disablez")) {
				result.disableZ = true;
			} else if (tokenizer.IsNamedIdentifier("double")) {
				result.faceCulling = false;
			} else if (tokenizer.IsNamedIdentifier("clamp")) {
				result.clamp = true;
			} else {
				if (mStrict) {
					throw CreateError("Unrecognized token '{0}'", tokenizer.TokenText.ToString());
				}
			}
		}

		return result;
	}

	private MdfMaterial ParseGeneral()
	{

		var result = new MdfMaterial(MdfType.General);

		var tokenizer = new Tokenizer(mContent);
		tokenizer.NextToken(); // Skip material type

		while (tokenizer.NextToken()) {

			if (!tokenizer.IsIdentifier) {
				if (mStrict) {
					throw CreateError("Unexpected token: {0}", tokenizer.TokenText.ToString());
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("highquality")) {
				// In no case is the GPU the bottleneck anymore,
				// so we will always parse the high quality section
				// Previously, it did cancel here if the GPU supported
				// less than 4 textures
				continue;
			}

			if (tokenizer.IsNamedIdentifier("texture")) {
				if (!ParseTextureStageId(ref tokenizer)) {
					continue;
				}

				var samplerNo = tokenizer.TokenInt;
				if (ParseFilename(ref tokenizer, "Texture")) {
					result.samplers[samplerNo].filename = tokenizer.TokenText.ToString();
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("glossmap")) {
				if (ParseFilename(ref tokenizer, "GlossMap")) {
					result.glossmap = tokenizer.TokenText.ToString();
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("uvtype")) {
				if (!ParseTextureStageId(ref tokenizer)) {
					continue;
				}

				var samplerNo = tokenizer.TokenInt;

				if (!ParseIdentifier(ref tokenizer, "UvType")) {
					continue;
				}

				MdfUvType uvType;
				if (tokenizer.IsNamedIdentifier("mesh")) {
					uvType = MdfUvType.Mesh;
				} else if (tokenizer.IsNamedIdentifier("environment")) {
					uvType = MdfUvType.Environment;
				} else if (tokenizer.IsNamedIdentifier("drift")) {
					uvType = MdfUvType.Drift;
				} else if (tokenizer.IsNamedIdentifier("swirl")) {
					uvType = MdfUvType.Swirl;
				} else if (tokenizer.IsNamedIdentifier("wavey")) {
					uvType = MdfUvType.Wavey;
				} else {
					if (mStrict) {
						throw CreateError("Unrecognized UvType: {0}", tokenizer.TokenText.ToString());
					}
					continue;
				}
				result.samplers[samplerNo].uvType = uvType;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("blendtype")) {
				if (!ParseTextureStageId(ref tokenizer)) {
					continue;
				}

				var samplerNo = tokenizer.TokenInt;

				if (!ParseIdentifier(ref tokenizer, "BlendType")) {
					continue;
				}

				MdfTextureBlendType blendType;
				if (tokenizer.IsNamedIdentifier("modulate")) {
					blendType = MdfTextureBlendType.Modulate;
				} else if (tokenizer.IsNamedIdentifier("add")) {
					blendType = MdfTextureBlendType.Add;
				} else if (tokenizer.IsNamedIdentifier("texturealpha")) {
					blendType = MdfTextureBlendType.TextureAlpha;
				} else if (tokenizer.IsNamedIdentifier("currentalpha")) {
					blendType = MdfTextureBlendType.CurrentAlpha;
				} else if (tokenizer.IsNamedIdentifier("currentalphaadd")) {
					blendType = MdfTextureBlendType.CurrentAlphaAdd;
				} else {
					if (mStrict) {
						throw CreateError("Unrecognized BlendType: {0}", tokenizer.TokenText.ToString());
					}
					continue;
				}
				result.samplers[samplerNo].blendType = blendType;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("color")) {
				uint argbColor;
				if (ParseRgba(ref tokenizer, "Color", out argbColor)) {
					result.diffuse = argbColor;
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("specular")) {
				uint argbColor;
				if (ParseRgba(ref tokenizer, "Specular", out argbColor)) {
					result.specular = argbColor;
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("specularpower")) {
				if (!tokenizer.NextToken()) {
					if (mStrict) {
						throw CreateError("Unexpected end of file after SpecularPower");
					}
				} else if (!tokenizer.IsNumber) {
					if (mStrict) {
						throw CreateError("Expected number after SpecularPower, but got: {0}",
							tokenizer.TokenText.ToString());
					}
				} else {
					result.specularPower = tokenizer.TokenFloat;
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("materialblendtype")) {
				if (!ParseIdentifier(ref tokenizer, "MaterialBlendType")) {
					continue;
				}

				if (tokenizer.IsNamedIdentifier("none")) {
					result.blendType = MdfBlendType.None;
				} else if (tokenizer.IsNamedIdentifier("alpha")) {
					result.blendType = MdfBlendType.Alpha;
				} else if (tokenizer.IsNamedIdentifier("add")) {
					result.blendType = MdfBlendType.Add;
				} else if (tokenizer.IsNamedIdentifier("alphaadd")) {
					result.blendType = MdfBlendType.AlphaAdd;
				} else {
					if (mStrict) {
						throw CreateError("Unrecognized MaterialBlendType: {0}",
							tokenizer.TokenText.ToString());
					}
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("speed")) {
				if (!ParseNumber(ref tokenizer, "Speed")) {
					continue;
				}

				var speed = tokenizer.TokenFloat * 60.0f;

				// Set the speed for all texture stages and for both U and V
				foreach (var sampler in result.samplers) {
					sampler.speedU = speed;
					sampler.speedV = speed;
				}
				continue;
			}

			if (tokenizer.IsNamedIdentifier("speedu")) {
				if (!ParseTextureStageId(ref tokenizer)) {
					continue;
				}

				var samplerNo = tokenizer.TokenInt;

				if (!ParseNumber(ref tokenizer, "SpeedU")) {
					continue;
				}

				var speed = tokenizer.TokenFloat * 60.0f;
				result.samplers[samplerNo].speedU = speed;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("speedv")) {
				if (!ParseTextureStageId(ref tokenizer)) {
					continue;
				}

				var samplerNo = tokenizer.TokenInt;

				if (!ParseNumber(ref tokenizer, "SpeedV")) {
					continue;
				}

				var speed = tokenizer.TokenFloat * 60.0f;
				result.samplers[samplerNo].speedV = speed;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("double")) {
				result.faceCulling = false;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("linearfiltering")) {
				result.linearFiltering = true;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("recalculatenormals")) {
				result.recalculateNormals = true;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("zfillonly")) {
				result.enableColorWrite = false;
				result.enableZWrite = true;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("colorfillonly")) {
				result.enableColorWrite = true;
				result.enableZWrite = false;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("notlit")) {
				result.notLit = true;
				continue;
			}

			if (tokenizer.IsNamedIdentifier("disablez")) {
				result.disableZ = true;
				continue;
			}

			if (mStrict) {
				throw CreateError("Unrecognized token: {0}", tokenizer.TokenText.ToString());
			}

		}

		return result;
	}

	private bool ParseTextureStageId(ref Tokenizer tokenizer)
	{
		if (!tokenizer.NextToken()) {
			if (mStrict) {
				throw CreateError("Missing argument for texture");
			}
			return false;
		}
		if (!tokenizer.IsNumber || tokenizer.TokenInt < 0 || tokenizer.TokenInt >= 4) {
			if (mStrict) {
				throw CreateError("Expected a texture stage between 0 and 3 as the second argument: {0}",
					tokenizer.TokenText.ToString());
			}
			return false;
		}
		return true;
	}

	private bool ParseFilename(ref Tokenizer tokenizer, string logMsg)
	{

		if (!tokenizer.NextToken()) {
			if (mStrict) {
				throw CreateError("Filename for {0} is missing.", logMsg);
			}
			return false;
		} else if (!tokenizer.IsQuotedString) {
			if (mStrict) {
				throw CreateError("Unexpected token instead of filename found for {0}: {1}",
					logMsg, tokenizer.TokenText.ToString());
			}
			return false;
		} else {
			return true;
		}
	}

	private bool ParseIdentifier(ref Tokenizer tokenizer, string logMsg)
	{

		if (!tokenizer.NextToken()) {
			if (mStrict) {
				throw CreateError("Identifier after {0} expected.", logMsg);
			}
			return false;
		}

		if (!tokenizer.IsIdentifier) {
			if (mStrict) {
				throw CreateError("Identifier after {0} expected, but got: {1}",
					logMsg, tokenizer.TokenText.ToString());
			}
			return false;
		}

		return true;
	}

	private bool ParseRgba(ref Tokenizer tokenizer, string logMsg, out uint argbOut)
	{

		// Color in the input is RGBA
		argbOut = 0; // The output is ARGB

		if (!tokenizer.NextToken() || !tokenizer.IsNumber) {
			if (!mStrict) {
				return false;
			}
			throw CreateError("Missing red component for {0}", logMsg);
		}
		argbOut |= (uint) ((tokenizer.TokenInt & 0xFF) << 16);

		if (!tokenizer.NextToken() || !tokenizer.IsNumber) {
			if (!mStrict) {
				return false;
			}
			throw CreateError("Missing green component for {0}", logMsg);
		}
		argbOut |= (uint)(tokenizer.TokenInt & 0xFF) << 8;

		if (!tokenizer.NextToken() || !tokenizer.IsNumber) {
			if (!mStrict) {
				return false;
			}
			throw CreateError("Missing blue component for {0}", logMsg);
		}
		argbOut |= (uint)(tokenizer.TokenInt & 0xFF);

		if (!tokenizer.NextToken() || !tokenizer.IsNumber) {
			if (!mStrict) {
				return false;
			}
			throw CreateError("Missing alpha component for {0}", logMsg);
		}
		argbOut |= (uint)(tokenizer.TokenInt & 0xFF) << 24;

		return true;
	}

	private bool ParseNumber(ref Tokenizer tokenizer, string logMsg)
	{
		if (!tokenizer.NextToken()) {
			if (mStrict) {
				throw CreateError("Unexpected end of file after {0}", logMsg);
			}
			return false;
		} else if (!tokenizer.IsNumber) {
			if (mStrict) {
				throw CreateError("Expected number after {0}, but got: {1}",
					logMsg, tokenizer.TokenText.ToString());
			}
			return false;
		}
		return true;
	}

	// Creates a better error message with context
	[StringFormatMethod("format")]
	private MdfException CreateError(string format, params object[] args)
	{
		return new MdfException(string.Format(format, args));
	}
}

internal class MdfException : Exception
{
	public MdfException(string message) : base(message)
	{
	}
}