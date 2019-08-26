#include "UIWidgetsTextInputPlugin.h"
#include "UIWidgetsMessageManager.h"
#include <Foundation/Foundation.h>
#include <UIKit/UIKit.h>

static const char _kTextAffinityDownstream[] = "TextAffinity.downstream";
static const char _kTextAffinityUpstream[] = "TextAffinity.upstream";

static UIKeyboardType ToUIKeyboardType(NSDictionary* type) {
    NSString* inputType = type[@"name"];
    if ([inputType isEqualToString:@"TextInputType.text"])
        return UIKeyboardTypeDefault;
    if ([inputType isEqualToString:@"TextInputType.multiline"])
        return UIKeyboardTypeDefault;
    if ([inputType isEqualToString:@"TextInputType.number"]) {
        if ([type[@"signed"] boolValue])
            return UIKeyboardTypeNumbersAndPunctuation;
        return UIKeyboardTypeDecimalPad;
    }
    if ([inputType isEqualToString:@"TextInputType.phone"])
        return UIKeyboardTypePhonePad;
    if ([inputType isEqualToString:@"TextInputType.emailAddress"])
        return UIKeyboardTypeEmailAddress;
    if ([inputType isEqualToString:@"TextInputType.url"])
        return UIKeyboardTypeURL;
    return UIKeyboardTypeDefault;
}

static UITextAutocapitalizationType ToUITextAutoCapitalizationType(NSDictionary* type) {
    NSString* textCapitalization = type[@"textCapitalization"];
    if ([textCapitalization isEqualToString:@"TextCapitalization.characters"]) {
        return UITextAutocapitalizationTypeAllCharacters;
    } else if ([textCapitalization isEqualToString:@"TextCapitalization.sentences"]) {
        return UITextAutocapitalizationTypeSentences;
    } else if ([textCapitalization isEqualToString:@"TextCapitalization.words"]) {
        return UITextAutocapitalizationTypeWords;
    }
    return UITextAutocapitalizationTypeNone;
}

static UIReturnKeyType ToUIReturnKeyType(NSString* inputType) {
    // Where did the term "unspecified" come from? iOS has a "default" and Android
    // has "unspecified." These 2 terms seem to mean the same thing but we need
    // to pick just one. "unspecified" was chosen because "default" is often a
    // reserved word in languages with switch statements (dart, java, etc).
    if ([inputType isEqualToString:@"TextInputAction.unspecified"])
        return UIReturnKeyDefault;
    
    if ([inputType isEqualToString:@"TextInputAction.done"])
        return UIReturnKeyDone;
    
    if ([inputType isEqualToString:@"TextInputAction.go"])
        return UIReturnKeyGo;
    
    if ([inputType isEqualToString:@"TextInputAction.send"])
        return UIReturnKeySend;
    
    if ([inputType isEqualToString:@"TextInputAction.search"])
        return UIReturnKeySearch;
    
    if ([inputType isEqualToString:@"TextInputAction.next"])
        return UIReturnKeyNext;
    
    if (@available(iOS 9.0, *))
        if ([inputType isEqualToString:@"TextInputAction.continueAction"])
            return UIReturnKeyContinue;
    
    if ([inputType isEqualToString:@"TextInputAction.join"])
        return UIReturnKeyJoin;
    
    if ([inputType isEqualToString:@"TextInputAction.route"])
        return UIReturnKeyRoute;
    
    if ([inputType isEqualToString:@"TextInputAction.emergencyCall"])
        return UIReturnKeyEmergencyCall;
    
    if ([inputType isEqualToString:@"TextInputAction.newline"])
        return UIReturnKeyDefault;
    
    // Present default key if bad input type is given.
    return UIReturnKeyDefault;
}

#pragma mark - UIWidgetsTextPosition

@implementation UIWidgetsTextPosition

+ (instancetype)positionWithIndex:(NSUInteger)index {
    return [[[UIWidgetsTextPosition alloc] initWithIndex:index] autorelease];
}

- (instancetype)initWithIndex:(NSUInteger)index {
    self = [super init];
    if (self) {
        _index = index;
    }
    return self;
}

@end

#pragma mark - UIWidgetsTextRange

@implementation UIWidgetsTextRange

+ (instancetype)rangeWithNSRange:(NSRange)range {
    return [[[UIWidgetsTextRange alloc] initWithNSRange:range] autorelease];
}

- (instancetype)initWithNSRange:(NSRange)range {
    self = [super init];
    if (self) {
        _range = range;
    }
    return self;
}

- (UITextPosition*)start {
    return [UIWidgetsTextPosition positionWithIndex:self.range.location];
}

- (UITextPosition*)end {
    return [UIWidgetsTextPosition positionWithIndex:self.range.location + self.range.length];
}

- (BOOL)isEmpty {
    return self.range.length == 0;
}

- (id)copyWithZone:(NSZone*)zone {
    return [[UIWidgetsTextRange allocWithZone:zone] initWithNSRange:self.range];
}

@end

@interface UIWidgetsTextInputView : UIView <UITextInput>

// UITextInput
@property(nonatomic, readonly) NSMutableString* text;
@property(nonatomic, readonly) NSMutableString* markedText;
@property(readwrite, copy) UITextRange* selectedTextRange;
@property(nonatomic, strong) UITextRange* markedTextRange;
@property(nonatomic, copy) NSDictionary* markedTextStyle;
@property(nonatomic, assign) id<UITextInputDelegate> inputDelegate;

// UITextInputTraits
@property(nonatomic) UITextAutocapitalizationType autocapitalizationType;
@property(nonatomic) UITextAutocorrectionType autocorrectionType;
@property(nonatomic) UITextSpellCheckingType spellCheckingType;
@property(nonatomic) BOOL enablesReturnKeyAutomatically;
@property(nonatomic) UIKeyboardAppearance keyboardAppearance;
@property(nonatomic) UIKeyboardType keyboardType;
@property(nonatomic) UIReturnKeyType returnKeyType;
@property(nonatomic, getter=isSecureTextEntry) BOOL secureTextEntry;

@property(nonatomic, assign) id<UIWidgetsTextInputDelegate> textInputDelegate;

@end

@implementation UIWidgetsTextInputView {
    int _textInputClient;
    const char* _selectionAffinity;
    UIWidgetsTextRange* _selectedTextRange;
}

@synthesize tokenizer = _tokenizer;

- (instancetype)init {
    self = [super init];
    
    if (self) {
        _textInputClient = 0;
        _selectionAffinity = _kTextAffinityUpstream;
        
        // UITextInput
        _text = [[NSMutableString alloc] init];
        _markedText = [[NSMutableString alloc] init];
        _selectedTextRange = [[UIWidgetsTextRange alloc] initWithNSRange:NSMakeRange(0, 0)];
        
        // UITextInputTraits
        _autocapitalizationType = UITextAutocapitalizationTypeSentences;
        _autocorrectionType = UITextAutocorrectionTypeDefault;
        _spellCheckingType = UITextSpellCheckingTypeDefault;
        _enablesReturnKeyAutomatically = NO;
        _keyboardAppearance = UIKeyboardAppearanceDefault;
        _keyboardType = UIKeyboardTypeDefault;
        _returnKeyType = UIReturnKeyDone;
        _secureTextEntry = NO;
    }
    
    return self;
}

- (void)dealloc {
    [_text release];
    [_markedText release];
    [_markedTextRange release];
    [_selectedTextRange release];
    [_tokenizer release];
    [super dealloc];
}

- (void)setTextInputClient:(int)client {
    _textInputClient = client;
}

- (void)setTextInputState:(NSDictionary*)state {
    NSString* newText = state[@"text"];
    BOOL textChanged = ![self.text isEqualToString:newText];
    if (textChanged) {
        [self.inputDelegate textWillChange:self];
        [self.text setString:newText];
    }

    NSInteger composingBase = [state[@"composingBase"] intValue];
    NSInteger composingExtent = [state[@"composingExtent"] intValue];
    NSRange composingRange = [self clampSelection:NSMakeRange(MIN(composingBase, composingExtent), ABS(composingBase - composingExtent)) forText:self.text];
    self.markedTextRange = composingRange.length > 0 ? [UIWidgetsTextRange rangeWithNSRange:composingRange] : nil;
    
    NSInteger selectionBase = [state[@"selectionBase"] intValue];
    NSInteger selectionExtent = [state[@"selectionExtent"] intValue];
    NSRange selectedRange = [self clampSelection:NSMakeRange(MIN(selectionBase, selectionExtent),
                                                             ABS(selectionBase - selectionExtent))
                                         forText:self.text];
    NSRange oldSelectedRange = [(UIWidgetsTextRange*)self.selectedTextRange range];
    if (selectedRange.location != oldSelectedRange.location ||
        selectedRange.length != oldSelectedRange.length) {
        [self.inputDelegate selectionWillChange:self];
        [self setSelectedTextRange:[UIWidgetsTextRange rangeWithNSRange:selectedRange]
                updateEditingState:NO];
        _selectionAffinity = _kTextAffinityDownstream;
        if ([state[@"selectionAffinity"] isEqualToString:@(_kTextAffinityUpstream)])
            _selectionAffinity = _kTextAffinityUpstream;
        [self.inputDelegate selectionDidChange:self];
    }
    
    if (textChanged) {
        [self.inputDelegate textDidChange:self];
        
        // For consistency with Android behavior, send an update to the framework.
        [self updateEditingState];
    }
}

- (NSRange)clampSelection:(NSRange)range forText:(NSString*)text {
    int start = MIN(MAX(range.location, 0), text.length);
    int length = MIN(range.length, text.length - start);
    return NSMakeRange(start, length);
}

#pragma mark - UIResponder Overrides

- (BOOL)canBecomeFirstResponder {
    return YES;
}

#pragma mark - UITextInput Overrides

- (id<UITextInputTokenizer>)tokenizer {
    if (_tokenizer == nil) {
        _tokenizer = [[UITextInputStringTokenizer alloc] initWithTextInput:self];
    }
    return _tokenizer;
}

- (UITextRange*)selectedTextRange {
    return [[_selectedTextRange copy] autorelease];
}

- (void)setSelectedTextRange:(UITextRange*)selectedTextRange {
    [self setSelectedTextRange:selectedTextRange updateEditingState:YES];
}

- (void)setSelectedTextRange:(UITextRange*)selectedTextRange updateEditingState:(BOOL)update {
    if (_selectedTextRange != selectedTextRange) {
        UITextRange* oldSelectedRange = _selectedTextRange;
        _selectedTextRange = [selectedTextRange copy];
        [oldSelectedRange release];
        
        if (update)
            [self updateEditingState];
    }
}

- (id)insertDictationResultPlaceholder {
    return @"";
}

- (void)removeDictationResultPlaceholder:(id)placeholder willInsertResult:(BOOL)willInsertResult {
}

- (NSString*)textInRange:(UITextRange*)range {
    NSRange textRange = ((UIWidgetsTextRange*)range).range;
    return [self.text substringWithRange:textRange];
}

- (void)replaceRange:(UITextRange*)range withText:(NSString*)text {
    NSRange replaceRange = ((UIWidgetsTextRange*)range).range;
    NSRange selectedRange = _selectedTextRange.range;
    // Adjust the text selection:
    // * reduce the length by the intersection length
    // * adjust the location by newLength - oldLength + intersectionLength
    NSRange intersectionRange = NSIntersectionRange(replaceRange, selectedRange);
    if (replaceRange.location <= selectedRange.location)
        selectedRange.location += text.length - replaceRange.length;
    if (intersectionRange.location != NSNotFound) {
        selectedRange.location += intersectionRange.length;
        selectedRange.length -= intersectionRange.length;
    }
    
    [self.text replaceCharactersInRange:[self clampSelection:replaceRange forText:self.text]
                             withString:text];
    [self setSelectedTextRange:[UIWidgetsTextRange rangeWithNSRange:[self clampSelection:selectedRange
                                                                                 forText:self.text]]
            updateEditingState:NO];
    
    [self updateEditingState];
}

- (BOOL)shouldChangeTextInRange:(UITextRange*)range replacementText:(NSString*)text {
    if (self.returnKeyType == UIReturnKeyDefault && [text isEqualToString:@"\n"]) {
        [_textInputDelegate performAction:UIWidgetsTextInputActionNewline withClient:_textInputClient];
        return YES;
    }
    
    if ([text isEqualToString:@"\n"]) {
        UIWidgetsTextInputAction action;
        switch (self.returnKeyType) {
            case UIReturnKeyDefault:
                action = UIWidgetsTextInputActionUnspecified;
                break;
            case UIReturnKeyDone:
                action = UIWidgetsTextInputActionDone;
                break;
            case UIReturnKeyGo:
                action = UIWidgetsTextInputActionGo;
                break;
            case UIReturnKeySend:
                action = UIWidgetsTextInputActionSend;
                break;
            case UIReturnKeySearch:
            case UIReturnKeyGoogle:
            case UIReturnKeyYahoo:
                action = UIWidgetsTextInputActionSearch;
                break;
            case UIReturnKeyNext:
                action = UIWidgetsTextInputActionNext;
                break;
            case UIReturnKeyContinue:
                action = UIWidgetsTextInputActionContinue;
                break;
            case UIReturnKeyJoin:
                action = UIWidgetsTextInputActionJoin;
                break;
            case UIReturnKeyRoute:
                action = UIWidgetsTextInputActionRoute;
                break;
            case UIReturnKeyEmergencyCall:
                action = UIWidgetsTextInputActionEmergencyCall;
                break;
        }
        
        [_textInputDelegate performAction:action withClient:_textInputClient];
        return NO;
    }
    
    return YES;
}

- (void)setMarkedText:(NSString*)markedText selectedRange:(NSRange)markedSelectedRange {
    NSRange selectedRange = _selectedTextRange.range;
    NSRange markedTextRange = ((UIWidgetsTextRange*)self.markedTextRange).range;
    
    if (markedText == nil)
        markedText = @"";
    
    if (markedTextRange.length > 0) {
        // Replace text in the marked range with the new text.
        [self replaceRange:self.markedTextRange withText:markedText];
        markedTextRange.length = markedText.length;
    } else {
        // Replace text in the selected range with the new text.
        [self replaceRange:_selectedTextRange withText:markedText];
        markedTextRange = NSMakeRange(selectedRange.location, markedText.length);
    }
    
    self.markedTextRange =
    markedTextRange.length > 0 ? [UIWidgetsTextRange rangeWithNSRange:markedTextRange] : nil;
    
    NSUInteger selectionLocation = markedSelectedRange.location + markedTextRange.location;
    selectedRange = NSMakeRange(selectionLocation, markedSelectedRange.length);
    [self setSelectedTextRange:[UIWidgetsTextRange rangeWithNSRange:[self clampSelection:selectedRange
                                                                                 forText:self.text]]
            updateEditingState:YES];
}

- (void)unmarkText {
    self.markedTextRange = nil;
    [self updateEditingState];
}

- (UITextRange*)textRangeFromPosition:(UITextPosition*)fromPosition
                           toPosition:(UITextPosition*)toPosition {
    NSUInteger fromIndex = ((UIWidgetsTextPosition*)fromPosition).index;
    NSUInteger toIndex = ((UIWidgetsTextPosition*)toPosition).index;
    return [UIWidgetsTextRange rangeWithNSRange:NSMakeRange(fromIndex, toIndex - fromIndex)];
}

/** Returns the range of the character sequence at the specified index in the
 * text. */
- (NSRange)rangeForCharacterAtIndex:(NSUInteger)index {
    if (index < self.text.length)
        return [self.text rangeOfComposedCharacterSequenceAtIndex:index];
    return NSMakeRange(index, 0);
}

- (NSUInteger)decrementOffsetPosition:(NSUInteger)position {
    return [self rangeForCharacterAtIndex:MAX(0, position - 1)].location;
}

- (NSUInteger)incrementOffsetPosition:(NSUInteger)position {
    NSRange charRange = [self rangeForCharacterAtIndex:position];
    return MIN(position + charRange.length, self.text.length);
}

- (UITextPosition*)positionFromPosition:(UITextPosition*)position offset:(NSInteger)offset {
    NSUInteger offsetPosition = ((UIWidgetsTextPosition*)position).index;
    if (offset >= 0) {
        for (NSInteger i = 0; i < offset && offsetPosition < self.text.length; ++i)
            offsetPosition = [self incrementOffsetPosition:offsetPosition];
    } else {
        for (NSInteger i = 0; i < ABS(offset) && offsetPosition > 0; ++i)
            offsetPosition = [self decrementOffsetPosition:offsetPosition];
    }
    return [UIWidgetsTextPosition positionWithIndex:offsetPosition];
}

- (UITextPosition*)positionFromPosition:(UITextPosition*)position
                            inDirection:(UITextLayoutDirection)direction
                                 offset:(NSInteger)offset {
    // TODO(cbracken) Add RTL handling.
    switch (direction) {
        case UITextLayoutDirectionLeft:
        case UITextLayoutDirectionUp:
            return [self positionFromPosition:position offset:offset * -1];
        case UITextLayoutDirectionRight:
        case UITextLayoutDirectionDown:
            return [self positionFromPosition:position offset:1];
    }
}

- (UITextPosition*)beginningOfDocument {
    return [UIWidgetsTextPosition positionWithIndex:0];
}

- (UITextPosition*)endOfDocument {
    return [UIWidgetsTextPosition positionWithIndex:self.text.length];
}

- (NSComparisonResult)comparePosition:(UITextPosition*)position toPosition:(UITextPosition*)other {
    NSUInteger positionIndex = ((UIWidgetsTextPosition*)position).index;
    NSUInteger otherIndex = ((UIWidgetsTextPosition*)other).index;
    if (positionIndex < otherIndex)
        return NSOrderedAscending;
    if (positionIndex > otherIndex)
        return NSOrderedDescending;
    return NSOrderedSame;
}

- (NSInteger)offsetFromPosition:(UITextPosition*)from toPosition:(UITextPosition*)toPosition {
    return ((UIWidgetsTextPosition*)toPosition).index - ((UIWidgetsTextPosition*)from).index;
}

- (UITextPosition*)positionWithinRange:(UITextRange*)range
                   farthestInDirection:(UITextLayoutDirection)direction {
    NSUInteger index;
    switch (direction) {
        case UITextLayoutDirectionLeft:
        case UITextLayoutDirectionUp:
            index = ((UIWidgetsTextPosition*)range.start).index;
            break;
        case UITextLayoutDirectionRight:
        case UITextLayoutDirectionDown:
            index = ((UIWidgetsTextPosition*)range.end).index;
            break;
    }
    return [UIWidgetsTextPosition positionWithIndex:index];
}

- (UITextRange*)characterRangeByExtendingPosition:(UITextPosition*)position
                                      inDirection:(UITextLayoutDirection)direction {
    NSUInteger positionIndex = ((UIWidgetsTextPosition*)position).index;
    NSUInteger startIndex;
    NSUInteger endIndex;
    switch (direction) {
        case UITextLayoutDirectionLeft:
        case UITextLayoutDirectionUp:
            startIndex = [self decrementOffsetPosition:positionIndex];
            endIndex = positionIndex;
            break;
        case UITextLayoutDirectionRight:
        case UITextLayoutDirectionDown:
            startIndex = positionIndex;
            endIndex = [self incrementOffsetPosition:positionIndex];
            break;
    }
    return [UIWidgetsTextRange rangeWithNSRange:NSMakeRange(startIndex, endIndex - startIndex)];
}

#pragma mark - UITextInput text direction handling

- (UITextWritingDirection)baseWritingDirectionForPosition:(UITextPosition*)position
                                              inDirection:(UITextStorageDirection)direction {
    return UITextWritingDirectionNatural;
}

- (void)setBaseWritingDirection:(UITextWritingDirection)writingDirection
                       forRange:(UITextRange*)range {
}

#pragma mark - UITextInput cursor, selection rect handling

- (CGRect)firstRectForRange:(UITextRange*)range {
    return CGRectZero;
}

- (CGRect)caretRectForPosition:(UITextPosition*)position {
    return CGRectZero;
}

- (UITextPosition*)closestPositionToPoint:(CGPoint)point {
    NSUInteger currentIndex = ((UIWidgetsTextPosition*)_selectedTextRange.start).index;
    return [UIWidgetsTextPosition positionWithIndex:currentIndex];
}

- (NSArray*)selectionRectsForRange:(UITextRange*)range {
    return @[];
}

- (UITextPosition*)closestPositionToPoint:(CGPoint)point withinRange:(UITextRange*)range {
    return range.start;
}

- (UITextRange*)characterRangeAtPoint:(CGPoint)point {
    NSUInteger currentIndex = ((UIWidgetsTextPosition*)_selectedTextRange.start).index;
    return [UIWidgetsTextRange rangeWithNSRange:[self rangeForCharacterAtIndex:currentIndex]];
}

#pragma mark - UIKeyInput Overrides

- (void)updateEditingState {
    NSUInteger selectionBase = ((UIWidgetsTextPosition*)_selectedTextRange.start).index;
    NSUInteger selectionExtent = ((UIWidgetsTextPosition*)_selectedTextRange.end).index;
    
    NSUInteger composingBase = 0;
    NSUInteger composingExtent = 0;
    if (self.markedTextRange != nil) {
        composingBase = ((UIWidgetsTextPosition*)self.markedTextRange.start).index;
        composingExtent = ((UIWidgetsTextPosition*)self.markedTextRange.end).index;
    }
    [_textInputDelegate updateEditingClient:_textInputClient
                                  withState:@{
                                              @"selectionBase" : @(selectionBase),
                                              @"selectionExtent" : @(selectionExtent),
                                              @"selectionAffinity" : @(_selectionAffinity),
                                              @"selectionIsDirectional" : @(false),
                                              @"composingBase" : @(composingBase),
                                              @"composingExtent" : @(composingExtent),
                                              @"text" : [NSString stringWithString:self.text],
                                              }];
}

- (BOOL)hasText {
    return self.text.length > 0;
}

- (void)insertText:(NSString*)text {
    _selectionAffinity = _kTextAffinityDownstream;
    [self replaceRange:_selectedTextRange withText:text];
}

- (void)deleteBackward {
    _selectionAffinity = _kTextAffinityDownstream;
    if (!_selectedTextRange.isEmpty)
        [self replaceRange:_selectedTextRange withText:@""];
}

@end

/**
 * Hides `UIWidgetsTextInputView` from iOS accessibility system so it
 * does not show up twice, once where it is in the `UIView` hierarchy,
 * and a second time as part of the `SemanticsObject` hierarchy.
 */
@interface UIWidgetsTextInputViewAccessibilityHider : UIView {
}

@end

@implementation UIWidgetsTextInputViewAccessibilityHider {
}

- (BOOL)accessibilityElementsHidden {
    return YES;
}

@end

@implementation UIWidgetsTextInputPlugin {
    UIWidgetsTextInputView* _view;
    UIWidgetsTextInputViewAccessibilityHider* _inputHider;
}

@synthesize textInputDelegate = _textInputDelegate;

- (instancetype)init {
    self = [super init];
    
    if (self) {
        _view = [[UIWidgetsTextInputView alloc] init];
        _inputHider = [[UIWidgetsTextInputViewAccessibilityHider alloc] init];
    }
    
    return self;
}

- (void)dealloc {
    [self hideTextInput];
    [_view release];
    [_inputHider release];
    
    [super dealloc];
}

- (UIView<UITextInput>*)textInputView {
    return _view;
}

- (void)showTextInput {
    NSAssert([UIApplication sharedApplication].keyWindow != nullptr,
             @"The application must have a key window since the keyboard client "
             @"must be part of the responder chain to function");
    _view.textInputDelegate = _textInputDelegate;
    [_inputHider addSubview:_view];
    [[UIApplication sharedApplication].keyWindow addSubview:_inputHider];
    [_view becomeFirstResponder];
}

- (void)hideTextInput {
    [_view resignFirstResponder];
    [_view removeFromSuperview];
    [_inputHider removeFromSuperview];
}

- (void)setTextInputClient:(int)client withConfiguration:(NSDictionary*)configuration {
    NSDictionary* inputType = configuration[@"inputType"];
    NSString* keyboardAppearance = configuration[@"keyboardAppearance"];
    _view.keyboardType = ToUIKeyboardType(inputType);
    _view.returnKeyType = ToUIReturnKeyType(configuration[@"inputAction"]);
    _view.autocapitalizationType = ToUITextAutoCapitalizationType(configuration);
    if ([keyboardAppearance isEqualToString:@"Brightness.dark"]) {
        _view.keyboardAppearance = UIKeyboardAppearanceDark;
    } else if ([keyboardAppearance isEqualToString:@"Brightness.light"]) {
        _view.keyboardAppearance = UIKeyboardAppearanceLight;
    } else {
        _view.keyboardAppearance = UIKeyboardAppearanceDefault;
    }
    _view.secureTextEntry = [configuration[@"obscureText"] boolValue];
    NSString* autocorrect = configuration[@"autocorrect"];
    _view.autocorrectionType = autocorrect && ![autocorrect boolValue]
    ? UITextAutocorrectionTypeNo
    : UITextAutocorrectionTypeDefault;
    [_view setTextInputClient:client];
    [_view reloadInputViews];
}

- (void)setTextInputEditingState:(NSDictionary*)state {
    [_view setTextInputState:state];
}

- (void)clearTextInputClient {
    [_view setTextInputClient:0];
}

+ (instancetype)sharedInstance {
    static UIWidgetsTextInputPlugin *sharedInstance = nil;
    static dispatch_once_t onceToken;
    
    dispatch_once(&onceToken, ^{
        sharedInstance = [[UIWidgetsTextInputPlugin alloc] init];
        sharedInstance.textInputDelegate = [[DefaultUIWidgetsTextInputDelegate alloc] init];
    });
    return sharedInstance;
}
@end


@implementation DefaultUIWidgetsTextInputDelegate

- (void)updateEditingClient:(int)client withState:(NSDictionary*)state {
    UIWidgetsMethodMessage(@"TextInput", @"TextInputClient.updateEditingState", @[@(client), state]);
}

- (void)performAction:(UIWidgetsTextInputAction)action withClient:(int)client {
    NSString* actionString;
    switch (action) {
        case UIWidgetsTextInputActionUnspecified:
            // Where did the term "unspecified" come from? iOS has a "default" and Android
            // has "unspecified." These 2 terms seem to mean the same thing but we need
            // to pick just one. "unspecified" was chosen because "default" is often a
            // reserved word in languages with switch statements (dart, java, etc).
            actionString = @"TextInputAction.unspecified";
            break;
        case UIWidgetsTextInputActionDone:
            actionString = @"TextInputAction.done";
            break;
        case UIWidgetsTextInputActionGo:
            actionString = @"TextInputAction.go";
            break;
        case UIWidgetsTextInputActionSend:
            actionString = @"TextInputAction.send";
            break;
        case UIWidgetsTextInputActionSearch:
            actionString = @"TextInputAction.search";
            break;
        case UIWidgetsTextInputActionNext:
            actionString = @"TextInputAction.next";
            break;
        case UIWidgetsTextInputActionContinue:
            actionString = @"TextInputAction.continue";
            break;
        case UIWidgetsTextInputActionJoin:
            actionString = @"TextInputAction.join";
            break;
        case UIWidgetsTextInputActionRoute:
            actionString = @"TextInputAction.route";
            break;
        case UIWidgetsTextInputActionEmergencyCall:
            actionString = @"TextInputAction.emergencyCall";
            break;
        case UIWidgetsTextInputActionNewline:
            actionString = @"TextInputAction.newline";
            break;
    }
    
    UIWidgetsMethodMessage(@"TextInput", @"TextInputClient.performAction", @[@(client), actionString]);
}
@end

extern "C" {
    
    void UIWidgetsTextInputShow() {
        [[UIWidgetsTextInputPlugin sharedInstance] showTextInput];
    }
    
    void UIWidgetsTextInputHide() {
        [[UIWidgetsTextInputPlugin sharedInstance] hideTextInput];
    }
    
    void UIWidgetsTextInputSetClient(int client, const char* configurationJson) {
        NSError *jsonError = nil;
        NSString *nsJsonString=[NSString stringWithUTF8String:configurationJson];
        NSData *objectData = [nsJsonString dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *json = [NSJSONSerialization JSONObjectWithData:objectData
                                                             options:NSJSONReadingMutableContainers
                                                               error:&jsonError];
        
        [[UIWidgetsTextInputPlugin sharedInstance] setTextInputClient:client withConfiguration:json];
    }
    
    void UIWidgetsTextInputSetTextInputEditingState(const char* jsonText) {
        NSError *jsonError;
        NSString *nsJsonString=[NSString stringWithUTF8String:jsonText];
        NSData *objectData = [nsJsonString dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *args = [NSJSONSerialization JSONObjectWithData:objectData
                                                             options:NSJSONReadingMutableContainers
                                                               error:&jsonError];
        [[UIWidgetsTextInputPlugin sharedInstance] setTextInputEditingState:args];
    }
    
    void UIWidgetsTextInputClearTextInputClient() {
        [[UIWidgetsTextInputPlugin sharedInstance] clearTextInputClient];
    }
}

