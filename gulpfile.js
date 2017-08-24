var gulp = require('gulp'),
sass = require('gulp-sass'),
gutil = require('gulp-util');

gulp.task('sass', function () {
return gulp.src('assets/styles/site.scss')
    .pipe( sass( { outputStyle: 'compressed' } ) )
    .pipe(gulp.dest('wwwroot/css'));
});

gulp.task('watch:sass', function () {
gulp.watch([
    './assets/styles/*.scss',
    '!./bin/**/*',
    '!./obj/**/*',
], {
    interval: 250
}, ['sass']).on('change', function (event) {
    gutil.log(`File ${event.path} was ${event.type}, running task.`);
})
})

gulp.task( 'default', ['watch:sass'] );
