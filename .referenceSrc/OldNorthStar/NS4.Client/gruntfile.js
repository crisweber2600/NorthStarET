/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
	// load Grunt plugins from NPM 
	grunt.loadNpmTasks('grunt-contrib-uglify');
	//
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-jshint');
	grunt.loadNpmTasks('grunt-contrib-clean');
	grunt.loadNpmTasks('grunt-autoprefixer');
	grunt.loadNpmTasks('grunt-bower-install');
	grunt.loadNpmTasks('grunt-rev');
	grunt.loadNpmTasks('grunt-usemin');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-imagemin');
	grunt.loadNpmTasks('grunt-svgmin');
	grunt.loadNpmTasks('grunt-contrib-htmlmin');
	grunt.loadNpmTasks('grunt-ngmin');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-google-cdn');
	grunt.loadNpmTasks('grunt-concurrent');
	grunt.loadNpmTasks('grunt-angular-templates');
	grunt.loadNpmTasks('grunt-contrib-less');
	grunt.loadNpmTasks('grunt-contrib-concat');
	
	//grunt.loadNpmTasks('grunt-karma');
	grunt.loadNpmTasks('grunt-processhtml'); 

	// Load grunt tasks automatically
	//require('load-grunt-tasks')(grunt);

	// Time how long tasks take. Can help when optimizing build times
	//require('time-grunt')(grunt);

	// configure plugins
	grunt.initConfig({
		yeoman: {
			// configurable paths
			app: require('./bower.json').appPath || 'app',
			dist: 'wwwroot'
		}, 

		watch: {
			//bower: {
			//	files: ['bower.json'],
			//	tasks: ['bowerInstall']
			//},
			js: {
				files: ['<%= yeoman.app %>/scripts/{,*/}*.js'],
				tasks: ['copy:js']
			},
			views: {
				files: ['<%= yeoman.app %>/views/{,*/}*.html'],
				tasks: ['copy:views']
			},
			//root: {
			//    files: ['<%= yeoman.app %>/index.html'],
			//    tasks: ['copy:root']
			//},
			//,
			//jsTest: {
			//	files: ['test/spec/{,*/}*.js'],
			//	tasks: ['newer:jshint:test', 'karma']
			//},
			styles: {
				files: ['<%= yeoman.app %>/styles/{,*/}*.css'],
				tasks: ['copy:nsstyles']
			},
			demostyles: {
			    files: ['<%= yeoman.app %>/assets/demo/variations/{,*/}*.css'],
			    tasks: ['copy:demostyles']
			},
			//less: {
			//	files: ['<%= yeoman.app %>/assets/less/*.less'],
			//	tasks: ['less:server']
			//},
			//gruntfile: {
			//	files: ['Gruntfile.js']
			//},
			//livereload: {
			//	options: {
			//		livereload: '<%= connect.options.livereload %>'
			//	},
			//	files: [
			//	  '<%= yeoman.app %>/{,*/}*.html',
			//	  '.tmp/assets/{,*/}*.css',
			//	  '<%= yeoman.app %>/images/{,*/}*.{png,jpg,jpeg,gif,webp,svg}'
			//	]
			//}
		},
		jshint: {
			options: {
				jshintrc: '.jshintrc',
				reporter: require('jshint-stylish')
			},
			all: [
			  'Gruntfile.js',
			  '<%= yeoman.app %>/scripts/{,*/}*.js'
			],
			test: {
				options: {
					jshintrc: 'test/.jshintrc'
				},
				src: ['test/spec/{,*/}*.js']
			}
		},

		// Empties folders to start fresh
		clean: {
			dist: {
				files: [{
					dot: true,
					src: [
					  '.tmp',
					  '<%= yeoman.dist %>/*',
					  '!<%= yeoman.dist %>/.git*'
					],
                    options: { force: true}
				}]
			},
			server: '.tmp'
		},

		// Add vendor prefixed styles
		autoprefixer: {
			options: {
				browsers: ['last 1 version']
			},
			dist: {
				files: [{
					expand: true,
					cwd: '.tmp/assets/css/',
					src: '{,*/}*.css',
					dest: '.tmp/assets/css/'
				}]
			}
		},
		// Automatically inject Bower components into the app
		bowerInstall: {
			app: {
				src: ['<%= yeoman.app %>/index.html'],
				ignorePath: '<%= yeoman.app %>/',
				exclude: ['requirejs',
						  'mocha',
						  'jquery.vmap.europe.js',
						  'jquery.vmap.usa.js',
						  'Chart.min.js',
						  'raphael',
						  'morris',
						  'jquery.inputmask',
						  'jquery.validate.js',
						  'jquery.stepy.js'
				]
			}
		},
		// Renames files for browser caching purposes
		rev: {
			dist: {
				files: {
					src: [
					  '<%= yeoman.dist %>/scripts/{,*/}*.js',
					  '<%= yeoman.dist %>/assets/css/{,*/}*.css',
					  '<%= yeoman.dist %>/images/{,*/}*.{png,jpg,jpeg,gif,webp,svg}',
					  '<%= yeoman.dist %>/styles/fonts/*'
					]
				}
			}
		},
		// Reads HTML for usemin blocks to enable smart builds that automatically
		// concat, minify and revision files. Creates configurations in memory so
		// additional tasks can operate on them
		useminPrepare: {
			html: '<%= yeoman.app %>/index.html',
			options: {
				dest: '<%= yeoman.dist %>',
				flow: {
					html: {
						steps: {
							js: ['concat', 'uglifyjs'],
							css: ['cssmin']
						},
						post: {}
					}
				}
			}
		},

		// Performs rewrites based on rev and the useminPrepare configuration
		usemin: {
			html: ['<%= yeoman.dist %>/{,*/}*.html'],
			css: ['<%= yeoman.dist %>/assets/css/{,*/}*.css'],
			options: {
				assetsDirs: ['<%= yeoman.dist %>']
			}
		},

		// The following *-min tasks produce minified files in the dist folder
		cssmin: {
			options: {
				// root: '<%= yeoman.app %>',
				relativeTo: '<%= yeoman.app %>',
				processImport: true,
				noAdvanced: true
			}
		},

		imagemin: {
			dist: {
				files: [{
					expand: true,
					cwd: '<%= yeoman.app %>/images',
					src: '{,*/}*.{png,jpg,jpeg,gif}',
					dest: '<%= yeoman.dist %>/images'
				}]
			}
		},

		svgmin: {
			dist: {
				files: [{
					expand: true,
					cwd: '<%= yeoman.app %>/images',
					src: '{,*/}*.svg',
					dest: '<%= yeoman.dist %>/images'
				}]
			}
		},

		htmlmin: {
			dist: {
				options: {
					collapseWhitespace: true,
					collapseBooleanAttributes: true,
					removeCommentsFromCDATA: true,
					removeOptionalTags: true
				},
				files: [{
					expand: true,
					cwd: '<%= yeoman.dist %>',
					src: ['*.html', 'views/{,*/}*.html'],
					dest: '<%= yeoman.dist %>'
				}]
			}
		},

		// ngmin tries to make the code safe for minification automatically by
		// using the Angular long form for dependency injection. It doesn't work on
		// things like resolve or inject so those have to be done manually.
		ngmin: {
			dist: {
				files: [{
					expand: true,
					cwd: '.tmp/concat/scripts',
					src: '*.js',
					dest: '.tmp/concat/scripts'
				}]
			}
		},

		// Replace Google CDN references
		cdnify: {
			dist: {
				html: ['<%= yeoman.dist %>/*.html']
			}
		},

		// Copies remaining files to places other tasks can use
		copy: {
			dist: {
				files: [{
					expand: true,
					dot: true,
					cwd: '<%= yeoman.app %>',
					dest: '<%= yeoman.dist %>',
					src: [
					  '*.{ico,png,txt}',
					  '.htaccess',
					  '*.html',
					  //'web.config',
					  'views/{,*/}*.html',
					  'images/{,*/}*.{webp}',
					  'fonts/*',
					  'assets/**',
					  'scripts/**',
					  'bower_components/jquery.inputmask/dist/jquery.inputmask.bundle.js',
					  'bower_components/jquery-validation/dist/jquery.validate.js',
					  'bower_components/jqvmap/jqvmap/maps/jquery.vmap.europe.js',
					  'bower_components/jqvmap/jqvmap/maps/jquery.vmap.usa.js',
					  'bower_components/stepy/lib/jquery.stepy.js',
					  'bower_components/Chart.js/Chart.min.js',
					  'bower_components/raphael/raphael.js',
					  'bower_components/morris.js/morris.js'
					]
				}, {
					expand: true,
					cwd: '.tmp/images',
					dest: '<%= yeoman.dist %>/images',
					src: ['generated/*']
				}]
			},
			styles: {
				expand: true,
				cwd: '.tmp/assets/css',
				dest: '<%= yeoman.app %>/assets/css',
				src: '{,*/}*.css'
			},
			nsstyles: {
			    expand: true,
			    cwd: '<%= yeoman.app %>/styles',
			    dest: '<%= yeoman.dist %>/styles',
			    src: '{,*/}*.css'
			},
			demostyles: {
			    expand: true,
			    cwd: '<%= yeoman.app %>/assets/demo/variations',
			    dest: '<%= yeoman.dist %>/assets/demo/variations',
			    src: '{,*/}*.css'
			},
			js: {
				expand: true,
				cwd: '<%= yeoman.app %>/scripts',
				dest: '<%= yeoman.dist %>/scripts',
				src: '{,*/}*.js'
			},
			views: {
				expand: true,
				cwd: '<%= yeoman.app %>/views',
				dest: '<%= yeoman.dist %>/views',
				src: '{,*/}*.html',
			},
			root: {
			    expand: true,
			    cwd: '<%= yeoman.app %>/',
			    dest: '<%= yeoman.dist %>/',
			    src: 'index.html',
			}
		},
		// Run some tasks in parallel to speed up the build process
		concurrent: {
			server: [
			  'copy:styles'
			],
			test: [
			  'copy:styles'
			],
			dist: [
			  'copy:styles',
			  'copy:dist',
			  // 'imagemin',
			  // 'svgmin'
			]
		},
		ngtemplates: {
			app: {
				src: 'app/views/templates/**.html',
				dest: 'app/scripts/templates/templates.js',
				options: {
					url: function (url) { return url.replace('app/views/', ''); },
					bootstrap: function (module, script) {
						return "angular.module('theme.templates', []).run(['$templateCache', function ($templateCache) {\n" + script + "}])";
					}
				},
			}
		},

		less: {
			server: {
				options: {
					// strictMath: true,
					dumpLineNumbers: true,
					sourceMap: true,
					sourceMapRootpath: "",
					outputSourceFiles: true
				},
				files: [
				  {
				  	expand: true,
				  	cwd: "<%= yeoman.app %>/assets/less",
				  	src: "styles.less",
				  	dest: ".tmp/assets/css",
				  	ext: ".css"
				  }
				]
			},
			dist: {
				options: {
					cleancss: true,
					report: 'min'
				},
				files: [
				  {
				  	expand: true,
				  	cwd: "<%= yeoman.app %>/assets/less",
				  	src: "styles.less",
				  	dest: ".tmp/assets/css",
				  	ext: ".css"
				  }
				]
			}
		},

		// By default, your `index.html`'s <!-- Usemin block --> will take care of
		// minification. These next options are pre-configured if you do not wish
		// to use the Usemin blocks.
		// cssmin: {
		//   dist: {
		//     files: {
		//       '<%= yeoman.dist %>/styles/main.css': [
		//         '.tmp/styles/{,*/}*.css',
		//         '<%= yeoman.app %>/styles/{,*/}*.css'
		//       ]
		//     }
		//   }
		// },
		// uglify: {
		//   dist: {
		//     files: {
		//       '<%= yeoman.dist %>/scripts/scripts.js': [
		//         '<%= yeoman.dist %>/scripts/scripts.js'
		//       ]
		//     }
		//   }
		// },
		 //concat: {
		 //  dist: {}
		 //},

		// Test settings
		karma: {
			unit: {
				configFile: 'karma.conf.js',
				singleRun: true
			}
		},
		processhtml: {
			options: {
				commentMarker: 'prochtml',
				process: true
			},
			dist: {
				files: {
					'<%= yeoman.dist %>/index.html': ['<%= yeoman.dist %>/index.html']
				}
			}
		},
		uglify: {
			options: {
				mangle: false
			}
		}

	});

	

	// define tasks
	grunt.registerTask('test', [
	'clean:server',
	'concurrent:test',
	'autoprefixer',
	'connect:test'
	//'karma'
		]);

	grunt.registerTask('build', [
	  //'clean:dist',
	  'bowerInstall',
	  'ngtemplates',
	  'useminPrepare',

	  'less:dist',
	  //'autoprefixer',
	  'concat',
	  'ngmin',

	  // 'cdnify',
	  'cssmin',
	  'uglify',
	  //'rev',
	  'usemin',
      	  'concurrent:dist',
      	  //'copy:dist',
	  'processhtml:dist'
	  // 'htmlmin'
	]);

	grunt.registerTask('default', ['build']);


};