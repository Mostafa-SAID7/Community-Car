/**
 * Tag Input Component
 * Handles tag input with autocomplete and suggestions
 */

(function() {
    'use strict';

    class TagInput {
        constructor(element, options = {}) {
            this.element = element;
            this.options = {
                maxTags: options.maxTags || 5,
                minLength: options.minLength || 2,
                maxLength: options.maxLength || 30,
                suggestions: options.suggestions || [],
                placeholder: options.placeholder || 'Add tags...',
                onTagAdd: options.onTagAdd || null,
                onTagRemove: options.onTagRemove || null,
                ...options
            };

            this.tags = [];
            this.init();
        }

        init() {
            this.createWrapper();
            this.createInput();
            this.createSuggestions();
            this.attachEvents();
            this.loadExistingTags();
        }

        createWrapper() {
            this.wrapper = document.createElement('div');
            this.wrapper.className = 'tag-input-wrapper';
            this.element.parentNode.insertBefore(this.wrapper, this.element);
            this.element.style.display = 'none';
        }

        createInput() {
            this.input = document.createElement('input');
            this.input.type = 'text';
            this.input.className = 'tag-input';
            this.input.placeholder = this.options.placeholder;
            this.input.maxLength = this.options.maxLength;
            this.wrapper.appendChild(this.input);
        }

        createSuggestions() {
            this.suggestionsContainer = document.createElement('div');
            this.suggestionsContainer.className = 'tag-suggestions';
            this.suggestionsContainer.style.display = 'none';
            this.wrapper.parentNode.appendChild(this.suggestionsContainer);
        }

        attachEvents() {
            this.input.addEventListener('keydown', (e) => this.handleKeyDown(e));
            this.input.addEventListener('input', (e) => this.handleInput(e));
            this.input.addEventListener('blur', () => {
                setTimeout(() => this.hideSuggestions(), 200);
            });

            document.addEventListener('click', (e) => {
                if (!this.wrapper.contains(e.target) && !this.suggestionsContainer.contains(e.target)) {
                    this.hideSuggestions();
                }
            });
        }

        handleKeyDown(e) {
            if (e.key === 'Enter' || e.key === ',') {
                e.preventDefault();
                this.addTag(this.input.value.trim());
            } else if (e.key === 'Backspace' && this.input.value === '' && this.tags.length > 0) {
                this.removeTag(this.tags[this.tags.length - 1]);
            }
        }

        handleInput(e) {
            const value = e.target.value.trim();
            
            if (value.length >= this.options.minLength) {
                this.showSuggestions(value);
            } else {
                this.hideSuggestions();
            }
        }

        addTag(tagName) {
            if (!tagName || tagName.length < this.options.minLength) {
                return;
            }

            // Normalize tag name
            tagName = tagName.toLowerCase().replace(/[^a-z0-9-]/g, '-');

            if (this.tags.includes(tagName)) {
                this.showError('Tag already added');
                return;
            }

            if (this.tags.length >= this.options.maxTags) {
                this.showError(`Maximum ${this.options.maxTags} tags allowed`);
                return;
            }

            this.tags.push(tagName);
            this.renderTag(tagName);
            this.updateHiddenInput();
            this.input.value = '';
            this.hideSuggestions();

            if (this.options.onTagAdd) {
                this.options.onTagAdd(tagName);
            }
        }

        removeTag(tagName) {
            const index = this.tags.indexOf(tagName);
            if (index > -1) {
                this.tags.splice(index, 1);
                this.updateHiddenInput();
                this.renderTags();

                if (this.options.onTagRemove) {
                    this.options.onTagRemove(tagName);
                }
            }
        }

        renderTag(tagName) {
            const tagElement = document.createElement('span');
            tagElement.className = 'tag';
            tagElement.innerHTML = `
                ${tagName}
                <span class="tag-remove" data-tag="${tagName}">&times;</span>
            `;

            tagElement.querySelector('.tag-remove').addEventListener('click', () => {
                this.removeTag(tagName);
            });

            this.wrapper.insertBefore(tagElement, this.input);
        }

        renderTags() {
            // Remove all existing tag elements
            this.wrapper.querySelectorAll('.tag').forEach(el => el.remove());
            
            // Re-render all tags
            this.tags.forEach(tag => this.renderTag(tag));
        }

        updateHiddenInput() {
            this.element.value = this.tags.join(',');
        }

        showSuggestions(query) {
            const filtered = this.options.suggestions.filter(suggestion => 
                suggestion.name.toLowerCase().includes(query.toLowerCase()) &&
                !this.tags.includes(suggestion.slug)
            );

            if (filtered.length === 0) {
                this.hideSuggestions();
                return;
            }

            this.suggestionsContainer.innerHTML = '';
            filtered.slice(0, 10).forEach(suggestion => {
                const item = document.createElement('div');
                item.className = 'tag-suggestion-item';
                item.innerHTML = `
                    <span class="tag-suggestion-name">${suggestion.name}</span>
                    <span class="tag-suggestion-count">${suggestion.usageCount || 0}</span>
                `;
                item.addEventListener('click', () => {
                    this.addTag(suggestion.slug);
                });
                this.suggestionsContainer.appendChild(item);
            });

            this.suggestionsContainer.style.display = 'block';
        }

        hideSuggestions() {
            this.suggestionsContainer.style.display = 'none';
        }

        showError(message) {
            // You can implement a toast notification here
            console.warn(message);
        }

        loadExistingTags() {
            const existingTags = this.element.value;
            if (existingTags) {
                existingTags.split(',').forEach(tag => {
                    const trimmed = tag.trim();
                    if (trimmed) {
                        this.tags.push(trimmed);
                        this.renderTag(trimmed);
                    }
                });
            }
        }

        getTags() {
            return this.tags;
        }

        setTags(tags) {
            this.tags = [];
            this.renderTags();
            tags.forEach(tag => this.addTag(tag));
        }

        destroy() {
            this.wrapper.remove();
            this.suggestionsContainer.remove();
            this.element.style.display = '';
        }
    }

    // Initialize all tag inputs on page load
    function initTagInputs() {
        document.querySelectorAll('[data-tag-input]').forEach(element => {
            const options = {
                maxTags: parseInt(element.dataset.maxTags) || 5,
                suggestions: window.tagSuggestions || []
            };
            new TagInput(element, options);
        });
    }

    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTagInputs);
    } else {
        initTagInputs();
    }

    // Expose to global scope
    window.TagInput = TagInput;

})();
