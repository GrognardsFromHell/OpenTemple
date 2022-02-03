namespace OpenTemple.Core.Particles.Instances;

public enum ParticleStateField {
    PSF_X = 0,
    PSF_Y,
    PSF_Z,
    PSF_POS_VAR_X,
    PSF_POS_VAR_Y,
    PSF_POS_VAR_Z,
    PSF_VEL_X,
    PSF_VEL_Y,
    PSF_VEL_Z,
    PSF_RED,
    PSF_GREEN,
    PSF_BLUE,
    PSF_ALPHA,
    // These seem to be used for polar coordinate based positioning
    PSF_POS_INCLINATION,
    PSF_POS_AZIMUTH,
    PSF_POS_RADIUS,
    PSF_ROTATION,
    PSF_COUNT
}