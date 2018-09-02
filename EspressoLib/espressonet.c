/*
** Filename: espressonet.c
**
** Interface to Espresso logic minimization engine
**
** Constants:
**     FTYPE
**     DTYPE
**     RTYPE
**
** Exceptions:
**     Error
**
** Interface Functions:
**     get_config
**     set_config
**     espresso
*/

#include "espresso.h"

/**
** Reference to cover data.
*/
typedef struct
{
    int ncubes;
    int ninputs;
    int noutput;
    int *data;
} net_cover_t;


/*
** Free's any data consumed by the net_cover_t struct.
*/

extern __declspec(dllexport)
void
espressonet_free(int *data)
{
    FREE(data);
}

/*
** Convert a .NET cover to an Espresso cover.
*/

static int
_netcov2esprcov(
    set_family_t *F, set_family_t *D, set_family_t *R,
    net_cover_t cover, int intype)
{
    int c, i, j;
    int index;
    int val, maxval;
    int savef, saved, saver;

    set *cf, *cd, *cr;

    /* Read cubes */
    cf = CUBE.temp[0];
    cd = CUBE.temp[1];
    cr = CUBE.temp[2];

    for (c = 0; c < cover.ncubes; c += 1) {
        set_clear(cf, CUBE.size);
        index = 0;

        // process inputs
        for (i = 0; i < cover.ninputs; i++) {
            val = cover.data[c * (cover.ninputs + cover.noutput) + i]; // value of input at i
            maxval = (1 << CUBE.part_size[i]) - 1;
            if (val < 0 || val > maxval) {
                // format("expected input in range [0, %d], got: %d", maxval, val);
                goto error;
            }

            for (j = 0; j < CUBE.part_size[i]; j++, index++) {
                if (val & (1 << j))
                    set_insert(cf, index);
            }
        }

        set_copy(cd, cf);
        set_copy(cr, cf);

        savef = saved = saver = 0;

        // process outputs
        for (i = 0; i < cover.noutput; i++, index++) {
            val = cover.data[c * (cover.ninputs + cover.noutput) + cover.ninputs + i]; // value of output at i
            switch (val) {
                /* on */
            case 1:
                if (intype & F_type) {
                    set_insert(cf, index);
                    savef = 1;
                }
                break;
                /* don't care */
            case 2:
                if (intype & D_type) {
                    set_insert(cd, index);
                    saved = 1;
                }
                break;
                /* off */
            case 0:
                if (intype & R_type) {
                    set_insert(cr, index);
                    saver = 1;
                }
                break;
            default:
                // format("expected output in {0, 1, 2}, got %d", val);
                goto error;
            }
        }

        if (savef) F = sf_addset(F, cf);
        if (saved) D = sf_addset(D, cd);
        if (saver) R = sf_addset(R, cr);

    }

    /* Success */
    return 1;

error:
    return 0;
}

/*
** Convert an Espresso cover to a .NET cover.
*/
static net_cover_t
_esprcov2netcov(int ninputs, int noutput, set_family_t *F)
{
    net_cover_t cover;
    set *last, *p;

    cover.ncubes = F->count;
    cover.ninputs = ninputs;
    cover.noutput = noutput;
    cover.data = (int*)malloc(F->count * (ninputs + noutput) * sizeof(int));

    int c = 0;
    int i = 0;

    foreach_set(F, last, p) {
        for (i = 0; i < ninputs; i++) {
            int val = GETINPUT(p, i);
            cover.data[c * (ninputs + noutput) + i] = val;
        }

        for (i = 0; i < noutput; i++) {
            int val = GETOUTPUT(p, i);
            cover.data[c * (ninputs + noutput) + ninputs + i] = val;
        }

        c++;
    }

    return cover;
    goto error;
error:
    FREE(cover.data);
    cover.ncubes = 0;
    cover.ninputs = 0;
    cover.noutput = 0;
    cover.data = NULL;
    return cover;
}

//
///*
//** Python function definition: espresso.get_config()
//*/
//PyDoc_STRVAR(_get_config_docstring,
//    "Return a dict of Espresso global configuration values."
//);
//
//static PyObject *
//_get_config(PyObject *self)
//{
//    return Py_BuildValue(
//        "{s:i, s:i, s:i, s:i, s:i, s:i, s:i}",
//
//        "single_expand", single_expand,
//        "remove_essential", remove_essential,
//        "force_irredundant", force_irredundant,
//        "unwrap_onset", unwrap_onset,
//        "recompute_onset", recompute_onset,
//        "use_super_gasp", use_super_gasp,
//
//        "skip_make_sparse", skip_make_sparse
//    );
//}
//
///*
//** Python function definition: espresso.set_config()
//*/
//PyDoc_STRVAR(_set_config_docstring,
//    "Set Espresso global configuration values.\n\
    //\n\
    //    Parameters\n\
    //    ----------\n\
    //    single_expand : bool\n\
    //        stop after first expand/irredundant\n\
    //\n\
    //    remove_essential : bool\n\
    //        remove essential primes\n\
    //\n\
    //    force_irredundant : bool\n\
    //        iterate make_sparse to force a minimal solution\n\
    //\n\
    //    unwrap_onset : bool\n\
    //        unwrap the function output part before first expand\n\
    //\n\
    //    recompute_onset : bool\n\
    //        recompute onset using the complement before starting\n\
    //\n\
    //    use_super_gasp : bool\n\
    //        use the super_gasp strategy rather than last_gasp\n\
    //\n\
    //    skip_make_sparse : bool\n\
    //        skip the make_sparse step\n\
    //    "
    //);
    //
    //static PyObject *
    //_set_config(PyObject *self, PyObject *args, PyObject *kwargs)
    //{
    //    static char *keywords[] = {
    //        "remove_essential", "single_expand", "use_super_gasp",
    //        "recompute_onset", "unwrap_onset", "force_irredundant",
    //        "skip_make_sparse",
    //        NULL
    //    };
    //
    //    PyArg_ParseTupleAndKeywords(
    //        args, kwargs, "|iiiiiii:set_config", keywords,
    //        &remove_essential, &single_expand, &use_super_gasp,
    //        &recompute_onset, &unwrap_onset, &force_irredundant,
    //        &skip_make_sparse
    //    );
    //
    //    Py_RETURN_NONE;
    //}

extern __declspec(dllexport)
net_cover_t
espressonet(net_cover_t cover, int intype)
{
    net_cover_t ret;
    ret.ncubes = 0;
    ret.ninputs = 0;
    ret.noutput = 0;
    ret.data = NULL;

    set_family_t *F, *Fsave;
    set_family_t *D;
    set_family_t *R;

    if (cover.ninputs <= 0) {
        // format(PyExc_ValueError, "expected ninputs > 0, got: %d", ninputs);
        goto error;
    }

    if (cover.noutput <= 0) {
        // format(PyExc_ValueError, "expected noutput > 0, got: %d", noutput);
        goto error;
    }

    if (!(intype & FR_type)) {
        // format(PyExc_ValueError, "expected intype in {f, r, fd, fr, dr, fdr}");
        goto error;
    }

    /* Initialize global CUBE dimensions */
    CUBE.num_binary_vars = cover.ninputs;
    CUBE.num_vars = cover.ninputs + 1;
    CUBE.part_size = (int *)malloc(CUBE.num_vars * sizeof(int));
    CUBE.part_size[CUBE.num_vars - 1] = cover.noutput;
    cube_setup();

    /* Initialize F^on, F^dc, F^off */
    F = sf_new(10, CUBE.size);
    D = sf_new(10, CUBE.size);
    R = sf_new(10, CUBE.size);

    if (!_netcov2esprcov(F, D, R, cover, intype))
        goto free_espresso;

    if (intype == F_type || intype == FD_type) {
        sf_free(R);
        R = complement(cube2list(F, D));
    }
    else if (intype == FR_type) {
        //set_family_t *X;
        sf_free(D);
        D = complement(cube2list(F, R));
        //X = d1merge(sf_join(F, R), CUBE.num_vars - 1);
        //D = complement(cube1list(X));
        //sf_free(X);
    }
    else if (intype == R_type || intype == DR_type) {
        sf_free(F);
        F = complement(cube2list(D, R));
    }

    Fsave = sf_save(F);
    F = espresso(F, D, R);
    int err = verify(F, Fsave, D);
    if (err) {
        // format(_error, "Espresso result verify failed");
        sf_free(Fsave);
        goto free_espresso;
    }
    sf_free(Fsave);

    /* Might return NULL */
    ret = _esprcov2netcov(cover.ninputs, cover.noutput, F);

free_espresso:
    sf_free(F);
    sf_free(D);
    sf_free(R);
    sf_cleanup();
    sm_cleanup();
    cube_setdown();
    free(CUBE.part_size);
    return ret;

error:
    FREE(ret.data);
    return ret;
}

