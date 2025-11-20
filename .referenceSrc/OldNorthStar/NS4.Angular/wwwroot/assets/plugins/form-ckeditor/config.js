/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here.
    // For complete reference see:
    // http://docs.ckeditor.com/#!/api/CKEDITOR.config

    // The toolbar groups arrangement, optimized for two toolbar rows.
    config.toolbarGroups = [
	//	{ name: 'clipboard',   groups: [ 'clipboard', 'undo' ] },
		{ name: 'editing', groups: ['spellchecker'] },
		//{ name: 'links' },
		{ name: 'insert' },
		//{ name: 'forms' },
		{ name: 'tools' },
		//{ name: 'document',	   groups: [ 'mode', 'document', 'doctools' ] },
		//{ name: 'others' },
		//'/',
		{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
		//{ name: 'paragraph',   groups: [ 'list', 'indent', 'blocks', 'align', 'bidi' ] },
		//{ name: 'styles' },
		//{ name: 'colors' },
		//{ name: 'about' }
    ];

    // Remove some buttons provided by the standard plugins, which are
    // not needed in the Standard(s) toolbar.
    config.removeButtons = 'Subscript,Superscript';

    // Set the most common block elements.
    config.format_tags = 'p;h1;h2;h3;pre';

    // Simplify the dialog windows.
    config.removeDialogTabs = 'image:advanced;link:advanced';


    config.imageUploadUrl = 'https://api.northstaret.net/api/fileuploader/uploadimages';
    config.uploadUrl = 'https://api.northstaret.net/api/fileuploader/uploadimages';
    //config.imageUploadUrl = 'http://localhost:16726/api/fileuploader/uploadimages';
    //config.uploadUrl = 'http://localhost:16726/api/fileuploader/uploadimages';

    // Configure your file manager integration. This example uses CKFinder 3 for PHP.
    //config.filebrowserBrowseUrl = '/ckfinder/ckfinder.html';
    //config.filebrowserImageBrowseUrl = '/ckfinder/ckfinder.html?type=Images';
    //config.filebrowserUploadUrl = '/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Files';

    //config.filebrowserImageUploadUrl = 'http://localhost:16726/api/fileuploader/uploadimagesjsResponse';
    config.filebrowserImageUploadUrl = 'https://api.northstaret.net/api/fileuploader/uploadimagesjsResponse';
    config.image_previewText = '';

};
