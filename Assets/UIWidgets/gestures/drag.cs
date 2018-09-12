namespace UIWidgets.gestures {
    public interface Drag {
        void update(DragUpdateDetails details);
        void end(DragEndDetails details);
        void cancel();
    }
}